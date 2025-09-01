# 🚀 Clase 8: Serialización Avanzada

## 📋 Información de la Clase

- **Módulo**: Junior Level 3 - Programación Orientada a Objetos Avanzada
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 7 (Reflection Avanzada)

## 🎯 Objetivos de Aprendizaje

- Dominar técnicas avanzadas de serialización en C#
- Implementar serialización personalizada con atributos
- Crear serializadores JSON y XML personalizados
- Utilizar serialización binaria y de contratos
- Implementar sistemas de persistencia avanzados

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia_multiple.md) | Herencia Múltiple y Composición | |
| [Clase 2](clase_2_interfaces_avanzadas.md) | Interfaces Avanzadas | |
| [Clase 3](clase_3_polimorfismo_avanzado.md) | Polimorfismo Avanzado | |
| [Clase 4](clase_4_patrones_diseno.md) | Patrones de Diseño Básicos | |
| [Clase 5](clase_5_principios_solid.md) | Principios SOLID | |
| [Clase 6](clase_6_arquitectura_modular.md) | Arquitectura Modular | |
| [Clase 7](clase_7_reflection_avanzada.md) | Reflection Avanzada | ← Anterior |
| **Clase 8** | **Serialización Avanzada** | ← Estás aquí |
| [Clase 9](clase_9_testing_unitario.md) | Testing Unitario | Siguiente → |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final Integrador | |

**← [Volver al README del Módulo 3](../junior_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Serialización JSON Avanzada

La serialización JSON avanzada permite control total sobre el proceso de conversión.

```csharp
// Clases de ejemplo para serialización
public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Email { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<Direccion> Direcciones { get; set; } = new();
    
    [JsonIgnore]
    public string Contraseña { get; set; }
    
    [JsonPropertyName("nombre_completo")]
    public string NombreCompleto => $"{Nombre} (ID: {Id})";
}

public class Direccion
{
    public string Calle { get; set; }
    public string Ciudad { get; set; }
    public string CodigoPostal { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Pais { get; set; }
}

// Serializador JSON personalizado
public class CustomJsonSerializer
{
    private readonly JsonSerializerOptions _options;
    
    public CustomJsonSerializer()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        // Convertidores personalizados
        _options.Converters.Add(new DateTimeConverter());
        _options.Converters.Add(new EnumConverter());
    }
    
    public string Serializar<T>(T objeto)
    {
        try
        {
            return JsonSerializer.Serialize(objeto, _options);
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error al serializar objeto: {ex.Message}", ex);
        }
    }
    
    public T Deserializar<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, _options);
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error al deserializar JSON: {ex.Message}", ex);
        }
    }
    
    public async Task<string> SerializarAsync<T>(T objeto, Stream stream)
    {
        try
        {
            await JsonSerializer.SerializeAsync(stream, objeto, _options);
            return "Serialización completada";
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error en serialización asíncrona: {ex.Message}", ex);
        }
    }
    
    public async Task<T> DeserializarAsync<T>(Stream stream)
    {
        try
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, _options);
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error en deserialización asíncrona: {ex.Message}", ex);
        }
    }
}

// Convertidor personalizado para DateTime
public class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (DateTime.TryParse(dateString, out DateTime result))
            {
                return result;
            }
        }
        
        return reader.GetDateTime();
    }
    
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
    }
}

// Convertidor personalizado para enums
public class EnumConverter : JsonConverter<Enum>
{
    public override Enum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var enumString = reader.GetString();
            return (Enum)Enum.Parse(typeToConvert, enumString, true);
        }
        
        return (Enum)Enum.ToObject(typeToConvert, reader.GetInt32());
    }
    
    public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

// Uso del serializador personalizado
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Serialización JSON Avanzada ===\n");
        
        var serializer = new CustomJsonSerializer();
        
        // Crear objeto de ejemplo
        var usuario = new Usuario
        {
            Id = 1,
            Nombre = "Juan Pérez",
            Email = "juan@email.com",
            FechaCreacion = DateTime.Now,
            Contraseña = "secret123",
            Direcciones = new List<Direccion>
            {
                new Direccion
                {
                    Calle = "Av. Principal 123",
                    Ciudad = "Ciudad de México",
                    CodigoPostal = "12345",
                    Pais = "México"
                },
                new Direccion
                {
                    Calle = "Calle Secundaria 456",
                    Ciudad = "Guadalajara",
                    CodigoPostal = "54321"
                }
            }
        };
        
        // Serializar a JSON
        var json = serializer.Serializar(usuario);
        Console.WriteLine("JSON Serializado:");
        Console.WriteLine(json);
        
        // Deserializar desde JSON
        var usuarioDeserializado = serializer.Deserializar<Usuario>(json);
        Console.WriteLine($"\nUsuario deserializado: {usuarioDeserializado.Nombre}");
        Console.WriteLine($"Direcciones: {usuarioDeserializado.Direcciones.Count}");
        
        // Serialización asíncrona a archivo
        var archivo = "usuario.json";
        using (var stream = File.Create(archivo))
        {
            var tarea = serializer.SerializarAsync(usuario, stream);
            tarea.Wait();
            Console.WriteLine($"\nArchivo guardado: {archivo}");
        }
        
        // Deserialización asíncrona desde archivo
        using (var stream = File.OpenRead(archivo))
        {
            var tarea = serializer.DeserializarAsync<Usuario>(stream);
            var usuarioArchivo = tarea.Result;
            Console.WriteLine($"Usuario desde archivo: {usuarioArchivo.Nombre}");
        }
    }
}
```

### 2. Serialización XML Avanzada

La serialización XML avanzada permite control sobre el formato y estructura del XML generado.

```csharp
// Clases con atributos de serialización XML
[XmlRoot("usuario", Namespace = "http://www.miapp.com/usuarios")]
public class UsuarioXML
{
    [XmlElement("identificador")]
    public int Id { get; set; }
    
    [XmlElement("nombre_completo")]
    public string Nombre { get; set; }
    
    [XmlElement("correo_electronico")]
    public string Email { get; set; }
    
    [XmlElement("fecha_creacion")]
    [XmlIgnore]
    public DateTime FechaCreacion { get; set; }
    
    [XmlElement("fecha_creacion_formateada")]
    public string FechaCreacionFormateada
    {
        get => FechaCreacion.ToString("yyyy-MM-dd");
        set => FechaCreacion = DateTime.Parse(value);
    }
    
    [XmlArray("direcciones")]
    [XmlArrayItem("direccion")]
    public List<DireccionXML> Direcciones { get; set; } = new();
    
    [XmlAttribute("activo")]
    public bool Activo { get; set; }
    
    [XmlIgnore]
    public string Contraseña { get; set; }
}

[XmlRoot("direccion")]
public class DireccionXML
{
    [XmlElement("calle")]
    public string Calle { get; set; }
    
    [XmlElement("ciudad")]
    public string Ciudad { get; set; }
    
    [XmlElement("codigo_postal")]
    public string CodigoPostal { get; set; }
    
    [XmlElement("pais")]
    public string Pais { get; set; }
    
    [XmlAttribute("tipo")]
    public TipoDireccion Tipo { get; set; }
}

public enum TipoDireccion
{
    [XmlEnum("casa")]
    Casa,
    [XmlEnum("trabajo")]
    Trabajo,
    [XmlEnum("otro")]
    Otro
}

// Serializador XML personalizado
public class CustomXmlSerializer
{
    private readonly XmlSerializerNamespaces _namespaces;
    private readonly XmlWriterSettings _writerSettings;
    private readonly XmlReaderSettings _readerSettings;
    
    public CustomXmlSerializer()
    {
        _namespaces = new XmlSerializerNamespaces();
        _namespaces.Add("app", "http://www.miapp.com/usuarios");
        _namespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        
        _writerSettings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };
        
        _readerSettings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true
        };
    }
    
    public string Serializar<T>(T objeto)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, _writerSettings);
            
            serializer.Serialize(xmlWriter, objeto, _namespaces);
            return stringWriter.ToString();
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error al serializar objeto a XML: {ex.Message}", ex);
        }
    }
    
    public T Deserializar<T>(string xml)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var stringReader = new StringReader(xml);
            using var xmlReader = XmlReader.Create(stringReader, _readerSettings);
            
            return (T)serializer.Deserialize(xmlReader);
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error al deserializar XML: {ex.Message}", ex);
        }
    }
    
    public async Task SerializarAsync<T>(T objeto, Stream stream)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var xmlWriter = XmlWriter.Create(stream, _writerSettings);
            
            serializer.Serialize(xmlWriter, objeto, _namespaces);
            await xmlWriter.FlushAsync();
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error en serialización XML asíncrona: {ex.Message}", ex);
        }
    }
    
    public async Task<T> DeserializarAsync<T>(Stream stream)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var xmlReader = XmlReader.Create(stream, _readerSettings);
            
            return (T)serializer.Deserialize(xmlReader);
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error en deserialización XML asíncrona: {ex.Message}", ex);
        }
    }
}

// Uso del serializador XML personalizado
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Serialización XML Avanzada ===\n");
        
        var serializer = new CustomXmlSerializer();
        
        // Crear objeto de ejemplo
        var usuario = new UsuarioXML
        {
            Id = 1,
            Nombre = "María García",
            Email = "maria@email.com",
            FechaCreacion = DateTime.Now,
            Activo = true,
            Contraseña = "secret456",
            Direcciones = new List<DireccionXML>
            {
                new DireccionXML
                {
                    Calle = "Calle Principal 789",
                    Ciudad = "Madrid",
                    CodigoPostal = "28001",
                    Pais = "España",
                    Tipo = TipoDireccion.Casa
                },
                new DireccionXML
                {
                    Calle = "Av. del Trabajo 321",
                    Ciudad = "Barcelona",
                    CodigoPostal = "08001",
                    Pais = "España",
                    Tipo = TipoDireccion.Trabajo
                }
            }
        };
        
        // Serializar a XML
        var xml = serializer.Serializar(usuario);
        Console.WriteLine("XML Serializado:");
        Console.WriteLine(xml);
        
        // Deserializar desde XML
        var usuarioDeserializado = serializer.Deserializar<UsuarioXML>(xml);
        Console.WriteLine($"\nUsuario deserializado: {usuarioDeserializado.Nombre}");
        Console.WriteLine($"Direcciones: {usuarioDeserializado.Direcciones.Count}");
        
        // Serialización asíncrona a archivo
        var archivo = "usuario.xml";
        using (var stream = File.Create(archivo))
        {
            var tarea = serializer.SerializarAsync(usuario, stream);
            tarea.Wait();
            Console.WriteLine($"\nArchivo XML guardado: {archivo}");
        }
    }
}
```

### 3. Serialización Binaria y de Contratos

La serialización binaria y de contratos permite control total sobre el proceso de serialización.

```csharp
// Clases con implementación de ISerializable
[Serializable]
public class UsuarioBinario : ISerializable
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Email { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<DireccionBinaria> Direcciones { get; set; }
    
    [NonSerialized]
    private string _contraseña;
    
    public string Contraseña
    {
        get => _contraseña;
        set => _contraseña = value;
    }
    
    // Constructor por defecto
    public UsuarioBinario()
    {
        Direcciones = new List<DireccionBinaria>();
    }
    
    // Constructor para deserialización
    protected UsuarioBinario(SerializationInfo info, StreamingContext context)
    {
        Id = info.GetInt32("Id");
        Nombre = info.GetString("Nombre");
        Email = info.GetString("Email");
        FechaCreacion = info.GetDateTime("FechaCreacion");
        Direcciones = (List<DireccionBinaria>)info.GetValue("Direcciones", typeof(List<DireccionBinaria>));
        _contraseña = info.GetString("Contraseña");
    }
    
    // Implementación de ISerializable
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Id", Id);
        info.AddValue("Nombre", Nombre);
        info.AddValue("Email", Email);
        info.AddValue("FechaCreacion", FechaCreacion);
        info.AddValue("Direcciones", Direcciones);
        info.AddValue("Contraseña", _contraseña);
    }
}

[Serializable]
public class DireccionBinaria : ISerializable
{
    public string Calle { get; set; }
    public string Ciudad { get; set; }
    public string CodigoPostal { get; set; }
    public string Pais { get; set; }
    
    public DireccionBinaria() { }
    
    protected DireccionBinaria(SerializationInfo info, StreamingContext context)
    {
        Calle = info.GetString("Calle");
        Ciudad = info.GetString("Ciudad");
        CodigoPostal = info.GetString("CodigoPostal");
        Pais = info.GetString("Pais");
    }
    
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Calle", Calle);
        info.AddValue("Ciudad", Ciudad);
        info.AddValue("CodigoPostal", CodigoPostal);
        info.AddValue("Pais", Pais);
    }
}

// Serializador binario personalizado
public class CustomBinarySerializer
{
    private readonly BinaryFormatter _formatter;
    
    public CustomBinarySerializer()
    {
        _formatter = new BinaryFormatter();
    }
    
    public byte[] Serializar(object objeto)
    {
        try
        {
            using var stream = new MemoryStream();
            _formatter.Serialize(stream, objeto);
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error en serialización binaria: {ex.Message}", ex);
        }
    }
    
    public T Deserializar<T>(byte[] datos)
    {
        try
        {
            using var stream = new MemoryStream(datos);
            return (T)_formatter.Deserialize(stream);
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error en deserialización binaria: {ex.Message}", ex);
        }
    }
    
    public async Task<byte[]> SerializarAsync(object objeto)
    {
        return await Task.Run(() => Serializar(objeto));
    }
    
    public async Task<T> DeserializarAsync<T>(byte[] datos)
    {
        return await Task.Run(() => Deserializar<T>(datos));
    }
}

// Serializador de contratos (DataContract)
[DataContract]
public class UsuarioContrato
{
    [DataMember(Name = "id", Order = 1)]
    public int Id { get; set; }
    
    [DataMember(Name = "nombre", Order = 2)]
    public string Nombre { get; set; }
    
    [DataMember(Name = "email", Order = 3)]
    public string Email { get; set; }
    
    [DataMember(Name = "fecha_creacion", Order = 4)]
    public DateTime FechaCreacion { get; set; }
    
    [DataMember(Name = "direcciones", Order = 5)]
    public List<DireccionContrato> Direcciones { get; set; } = new();
    
    // Propiedad no serializada
    public string Contraseña { get; set; }
}

[DataContract]
public class DireccionContrato
{
    [DataMember(Name = "calle")]
    public string Calle { get; set; }
    
    [DataMember(Name = "ciudad")]
    public string Ciudad { get; set; }
    
    [DataMember(Name = "codigo_postal")]
    public string CodigoPostal { get; set; }
    
    [DataMember(Name = "pais")]
    public string Pais { get; set; }
}

// Serializador de contratos personalizado
public class CustomContractSerializer
{
    private readonly DataContractSerializer _serializer;
    private readonly DataContractJsonSerializer _jsonSerializer;
    
    public CustomContractSerializer()
    {
        _serializer = new DataContractSerializer(typeof(UsuarioContrato));
        _jsonSerializer = new DataContractJsonSerializer(typeof(UsuarioContrato));
    }
    
    public string SerializarXml(object objeto)
    {
        try
        {
            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true });
            
            _serializer.WriteObject(xmlWriter, objeto);
            return stringWriter.ToString();
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error en serialización de contrato XML: {ex.Message}", ex);
        }
    }
    
    public T DeserializarXml<T>(string xml)
    {
        try
        {
            using var stringReader = new StringReader(xml);
            using var xmlReader = XmlReader.Create(stringReader);
            
            return (T)_serializer.ReadObject(xmlReader);
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error en deserialización de contrato XML: {ex.Message}", ex);
        }
    }
    
    public string SerializarJson(object objeto)
    {
        try
        {
            using var stream = new MemoryStream();
            _jsonSerializer.WriteObject(stream, objeto);
            
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error en serialización de contrato JSON: {ex.Message}", ex);
        }
    }
    
    public T DeserializarJson<T>(string json)
    {
        try
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            
            stream.Position = 0;
            return (T)_jsonSerializer.ReadObject(stream);
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error en deserialización de contrato JSON: {ex.Message}", ex);
        }
    }
}

// Uso de los serializadores
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Serialización Binaria y de Contratos ===\n");
        
        // Serialización binaria
        var binarySerializer = new CustomBinarySerializer();
        
        var usuarioBinario = new UsuarioBinario
        {
            Id = 1,
            Nombre = "Carlos López",
            Email = "carlos@email.com",
            FechaCreacion = DateTime.Now,
            Contraseña = "secret789",
            Direcciones = new List<DireccionBinaria>
            {
                new DireccionBinaria
                {
                    Calle = "Calle Nueva 456",
                    Ciudad = "Valencia",
                    CodigoPostal = "46001",
                    Pais = "España"
                }
            }
        };
        
        var datosBinarios = binarySerializer.Serializar(usuarioBinario);
        Console.WriteLine($"Datos binarios generados: {datosBinarios.Length} bytes");
        
        var usuarioBinarioDeserializado = binarySerializer.Deserializar<UsuarioBinario>(datosBinarios);
        Console.WriteLine($"Usuario binario deserializado: {usuarioBinarioDeserializado.Nombre}");
        
        // Serialización de contratos
        var contractSerializer = new CustomContractSerializer();
        
        var usuarioContrato = new UsuarioContrato
        {
            Id = 2,
            Nombre = "Ana Martínez",
            Email = "ana@email.com",
            FechaCreacion = DateTime.Now,
            Contraseña = "secret012",
            Direcciones = new List<DireccionContrato>
            {
                new DireccionContrato
                {
                    Calle = "Av. Central 789",
                    Ciudad = "Sevilla",
                    CodigoPostal = "41001",
                    Pais = "España"
                }
            }
        };
        
        var xmlContrato = contractSerializer.SerializarXml(usuarioContrato);
        Console.WriteLine("\nXML de contrato generado:");
        Console.WriteLine(xmlContrato);
        
        var jsonContrato = contractSerializer.SerializarJson(usuarioContrato);
        Console.WriteLine("\nJSON de contrato generado:");
        Console.WriteLine(jsonContrato);
        
        // Deserialización
        var usuarioContratoDeserializado = contractSerializer.DeserializarXml<UsuarioContrato>(xmlContrato);
        Console.WriteLine($"\nUsuario contrato deserializado: {usuarioContratoDeserializado.Nombre}");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Serializador de Configuración
Crea un serializador que permita guardar y cargar configuraciones en diferentes formatos.

### Ejercicio 2: Sistema de Logs Serializados
Implementa un sistema de logs que serialice entradas en formato JSON o XML.

### Ejercicio 3: Persistencia de Objetos Complejos
Desarrolla un sistema que permita persistir objetos complejos usando serialización binaria.

## 🔍 Puntos Clave

1. **Serialización JSON** con control total sobre el formato y conversiones
2. **Serialización XML** con atributos personalizados y namespaces
3. **Serialización binaria** implementando ISerializable para control completo
4. **Serialización de contratos** usando DataContract para servicios web
5. **Convertidores personalizados** para tipos especiales como DateTime y enums

## 📚 Recursos Adicionales

- [JSON Serialization - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview)
- [XML Serialization - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/standard/serialization/xml-and-soap-serialization)
- [Data Contracts - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/using-data-contracts)

---

**🎯 ¡Has completado la Clase 8! Ahora dominas la serialización avanzada en C#**

**📚 [Siguiente: Clase 9 - Testing Unitario](clase_9_testing_unitario.md)**
