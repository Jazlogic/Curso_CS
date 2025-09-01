# Clase 6: Strings y Texto en C#

## 🎯 Objetivos de la Clase
- Comprender qué son los strings y cómo se manejan en C#
- Aprender diferentes formas de concatenar y formatear texto
- Dominar los métodos más importantes de la clase String
- Entender el uso de StringBuilder para manipulación eficiente

---

## 📚 Navegación del Módulo 1

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_introduccion.md) | Introducción a C# y .NET | |
| [Clase 2](clase_2_variables_tipos.md) | Variables y Tipos de Datos | |
| [Clase 3](clase_3_operadores.md) | Operadores y Expresiones | |
| [Clase 4](clase_4_estructuras_control.md) | Estructuras de Control | |
| [Clase 5](clase_5_colecciones.md) | Colecciones | ← Anterior |
| **Clase 6** | **Manipulación de Strings** | ← Estás aquí |
| [Clase 7](clase_7_funciones.md) | Métodos y Funciones | Siguiente → |
| [Clase 8](clase_8_namespaces.md) | Namespaces y Organización | |
| [Clase 9](clase_9_manejo_errores.md) | Manejo de Errores | |
| [Clase 10](clase_10_poo_basica.md) | Programación Orientada a Objetos Básica | |

**← [Volver al README del Módulo 1](../junior_1/README.md)**

## 📚 Contenido Teórico

### 1. ¿Qué son los Strings?

Los strings son **secuencias de caracteres Unicode** que representan texto. En C#, los strings son objetos inmutables, lo que significa que una vez creados, no pueden modificarse. Cuando "modificas" un string, en realidad estás creando uno nuevo.

#### Características de los Strings:
- **Inmutables**: No se pueden cambiar después de la creación
- **Referencia**: Son tipos de referencia, no tipos de valor
- **Unicode**: Soportan caracteres de diferentes idiomas
- **Indexados**: Se puede acceder a caracteres individuales por posición

### 2. Creación y Declaración de Strings

#### 2.1 Formas Básicas de Crear Strings

```csharp
// Declaración simple
string nombre = "Juan Pérez";              // String literal
string apellido = "García";                // String literal
string direccion = "Calle Principal 123";  // String literal

// Declaración con caracteres especiales
string textoConComillas = "Él dijo \"Hola\"";        // Comillas escapadas
string textoConSaltoLinea = "Primera línea\nSegunda línea"; // Salto de línea
string textoConTab = "Columna1\tColumna2\tColumna3"; // Tabulación

// String vacío
string stringVacio = "";                    // String sin caracteres
string stringNull = null;                   // Referencia nula

// String con caracteres Unicode
string textoEspanol = "Español: áéíóúñ";   // Caracteres acentuados
string emoji = "😀🎉🚀";                    // Emojis

Console.WriteLine($"Nombre: {nombre}");
Console.WriteLine($"Apellido: {apellido}");
Console.WriteLine($"Dirección: {direccion}");
Console.WriteLine($"Texto con comillas: {textoConComillas}");
Console.WriteLine($"Texto con salto de línea:\n{textoConSaltoLinea}");
Console.WriteLine($"Texto con tab:\n{textoConTab}");
Console.WriteLine($"Texto en español: {textoEspanol}");
Console.WriteLine($"Emojis: {emoji}");
```

#### Explicación Línea por Línea:

**Línea 2: `string nombre = "Juan Pérez";`**
- `string` es el tipo de dato para texto
- `"Juan Pérez"` es un string literal (texto entre comillas dobles)
- Los espacios se incluyen en el string

**Línea 6: `string textoConComillas = "Él dijo \"Hola\"";`**
- `\"` es un carácter de escape para incluir comillas dentro del string
- El resultado es: `Él dijo "Hola"`

**Línea 7: `string textoConSaltoLinea = "Primera línea\nSegunda línea";`**
- `\n` es un carácter de escape para salto de línea
- Al imprimir, se verá en dos líneas separadas

**Línea 8: `string textoConTab = "Columna1\tColumna2\tColumna3";`**
- `\t` es un carácter de escape para tabulación
- Útil para crear columnas alineadas

**Línea 11: `string stringVacio = "";`**
- String sin caracteres (longitud 0)
- Diferente de `null`

**Línea 12: `string stringNull = null;`**
- Referencia que no apunta a ningún objeto
- Causará error si intentas usar métodos en él

#### 2.2 Caracteres de Escape Comunes

```csharp
Console.WriteLine("Caracteres de escape:");
Console.WriteLine("\\n = Salto de línea");
Console.WriteLine("\\t = Tabulación");
Console.WriteLine("\\\" = Comilla doble");
Console.WriteLine("\\' = Comilla simple");
Console.WriteLine("\\\\ = Barra invertida");
Console.WriteLine("\\r = Retorno de carro");
Console.WriteLine("\\b = Retroceso");

// Ejemplos prácticos
string ruta = "C:\\Users\\Usuario\\Documentos"; // Ruta de Windows
string cita = "Einstein dijo: \"La imaginación es más importante que el conocimiento\"";
string formato = "Nombre:\tJuan\nEdad:\t25\nCiudad:\tMadrid";

Console.WriteLine($"\nRuta: {ruta}");
Console.WriteLine($"Cita: {cita}");
Console.WriteLine($"Formato:\n{formato}");
```

### 3. Concatenación de Strings

La concatenación es el proceso de unir múltiples strings en uno solo.

#### 3.1 Operador + para Concatenación

```csharp
string nombre = "Juan";
string apellido = "Pérez";
int edad = 25;

// Concatenación básica con +
string nombreCompleto = nombre + " " + apellido;
string presentacion = nombreCompleto + " tiene " + edad + " años";

Console.WriteLine($"Nombre completo: {nombreCompleto}");
Console.WriteLine($"Presentación: {presentacion}");

// Concatenación con otros tipos
string mensaje = "El resultado es: " + 42;           // String + int
string precio = "Precio: $" + 19.99;                 // String + double
string booleano = "¿Es verdadero? " + true;          // String + bool

Console.WriteLine($"Mensaje: {mensaje}");
Console.WriteLine($"Precio: {precio}");
Console.WriteLine($"Booleano: {booleano}");
```

#### Explicación de la Concatenación con +:

**Línea 5: `string nombreCompleto = nombre + " " + apellido;`**
- `nombre` ("Juan") + `" "` (espacio) + `apellido` ("Pérez")
- Resultado: `"Juan Pérez"`

**Línea 6: `string presentacion = nombreCompleto + " tiene " + edad + " años";`**
- `nombreCompleto` ("Juan Pérez") + `" tiene "` + `edad` (25) + `" años"`
- Resultado: `"Juan Pérez tiene 25 años"`

**Línea 12: `string mensaje = "El resultado es: " + 42;`**
- C# convierte automáticamente el int 42 a string
- Resultado: `"El resultado es: 42"`

#### 3.2 String Interpolation (Interpolación de Strings)

La interpolación de strings es una forma más moderna y legible de crear strings:

```csharp
string nombre = "María";
string apellido = "García";
int edad = 30;
double altura = 1.65;

// Interpolación básica
string presentacion = $"Me llamo {nombre} {apellido}";
string informacion = $"Tengo {edad} años y mido {altura} metros";

Console.WriteLine(presentacion);
Console.WriteLine(informacion);

// Interpolación con expresiones
string mensaje = $"El año que viene tendré {edad + 1} años";
string comparacion = $"Mi altura en centímetros es: {altura * 100:F0} cm";

Console.WriteLine(mensaje);
Console.WriteLine(comparacion);

// Interpolación con formato
string precio = $"Precio: ${19.99:F2}";              // 2 decimales
string porcentaje = $"Porcentaje: {0.85:P1}";        // Formato de porcentaje
string fecha = $"Fecha: {DateTime.Now:yyyy-MM-dd}";  // Formato de fecha

Console.WriteLine(precio);
Console.WriteLine(porcentaje);
Console.WriteLine(fecha);
```

#### Explicación de la Interpolación:

**Línea 6: `string presentacion = $"Me llamo {nombre} {apellido}";`**
- `$` indica que es un string interpolado
- `{nombre}` se reemplaza con el valor de la variable `nombre`
- `{apellido}` se reemplaza con el valor de la variable `apellido`
- Resultado: `"Me llamo María García"`

**Línea 12: `string mensaje = $"El año que viene tendré {edad + 1} años";`**
- `{edad + 1}` evalúa la expresión `30 + 1 = 31`
- Resultado: `"El año que viene tendré 31 años"`

**Línea 16: `string precio = $"Precio: ${19.99:F2}";`**
- `:F2` formatea el número con 2 decimales
- Resultado: `"Precio: $19.99"`

**Línea 17: `string porcentaje = $"Porcentaje: {0.85:P1}";`**
- `:P1` formatea como porcentaje con 1 decimal
- Resultado: `"Porcentaje: 85.0%"`

#### 3.3 String.Format y String.Concat

```csharp
string nombre = "Carlos";
int edad = 28;
string ciudad = "Barcelona";

// String.Format
string mensaje1 = String.Format("Hola, me llamo {0} y tengo {1} años", nombre, edad);
string mensaje2 = String.Format("Vivo en {0}", ciudad);

// String.Concat
string mensaje3 = String.Concat("Nombre: ", nombre, ", Edad: ", edad.ToString());

Console.WriteLine(mensaje1);
Console.WriteLine(mensaje2);
Console.WriteLine(mensaje3);

// Comparación de métodos
string resultado1 = nombre + " " + edad;                    // Concatenación con +
string resultado2 = $"{nombre} {edad}";                     // Interpolación
string resultado3 = String.Format("{0} {1}", nombre, edad); // String.Format
string resultado4 = String.Concat(nombre, " ", edad);       // String.Concat

Console.WriteLine($"Resultado 1: {resultado1}");
Console.WriteLine($"Resultado 2: {resultado2}");
Console.WriteLine($"Resultado 3: {resultado3}");
Console.WriteLine($"Resultado 4: {resultado4}");
```

#### Explicación de String.Format y String.Concat:

**Línea 6: `String.Format("Hola, me llamo {0} y tengo {1} años", nombre, edad)`**
- `{0}` se reemplaza con el primer parámetro (`nombre`)
- `{1}` se reemplaza con el segundo parámetro (`edad`)
- Resultado: `"Hola, me llamo Carlos y tengo 28 años"`

**Línea 10: `String.Concat("Nombre: ", nombre, ", Edad: ", edad.ToString())`**
- Concatena todos los parámetros en orden
- `edad.ToString()` convierte el int a string
- Resultado: `"Nombre: Carlos, Edad: 28"`

### 4. Propiedades y Métodos de String

#### 4.1 Propiedades Básicas

```csharp
string texto = "Hola Mundo C#";

Console.WriteLine($"Texto original: \"{texto}\"");
Console.WriteLine($"Longitud: {texto.Length}");
Console.WriteLine($"¿Está vacío? {string.IsNullOrEmpty(texto)}");
Console.WriteLine($"¿Es null o espacios en blanco? {string.IsNullOrWhiteSpace(texto)}");

// String vacío para comparar
string textoVacio = "";
Console.WriteLine($"\nTexto vacío: \"{textoVacio}\"");
Console.WriteLine($"Longitud: {textoVacio.Length}");
Console.WriteLine($"¿Está vacío? {string.IsNullOrEmpty(textoVacio)}");
Console.WriteLine($"¿Es null o espacios en blanco? {string.IsNullOrWhiteSpace(textoVacio)}");

// String con solo espacios
string textoEspacios = "   ";
Console.WriteLine($"\nTexto con espacios: \"{textoEspacios}\"");
Console.WriteLine($"Longitud: {textoEspacios.Length}");
Console.WriteLine($"¿Está vacío? {string.IsNullOrEmpty(textoEspacios)}");
Console.WriteLine($"¿Es null o espacios en blanco? {string.IsNullOrWhiteSpace(textoEspacios)}");
```

#### Explicación de las Propiedades:

**Línea 1: `string texto = "Hola Mundo C#";`**
- String con 13 caracteres (incluyendo espacios)

**Línea 4: `texto.Length`**
- Propiedad que retorna el número de caracteres
- Resultado: `13`

**Línea 5: `string.IsNullOrEmpty(texto)`**
- Método estático que verifica si el string es null o vacío
- Resultado: `false` (porque tiene contenido)

**Línea 6: `string.IsNullOrWhiteSpace(texto)`**
- Método estático que verifica si el string es null, vacío o solo espacios
- Resultado: `false` (porque tiene contenido real)

#### 4.2 Métodos de Búsqueda y Comparación

```csharp
string texto = "Hola Mundo C# Programming";

Console.WriteLine($"Texto original: \"{texto}\"");

// Búsqueda de caracteres
int posicionH = texto.IndexOf('H');           // Primera ocurrencia de 'H'
int posicionMundo = texto.IndexOf("Mundo");   // Primera ocurrencia de "Mundo"
int posicionUltimaO = texto.LastIndexOf('o'); // Última ocurrencia de 'o'

Console.WriteLine($"Posición de 'H': {posicionH}");
Console.WriteLine($"Posición de 'Mundo': {posicionMundo}");
Console.WriteLine($"Posición de última 'o': {posicionUltimaO}");

// Verificación de contenido
bool contieneHola = texto.Contains("Hola");           // ¿Contiene "Hola"?
bool empiezaConHola = texto.StartsWith("Hola");       // ¿Empieza con "Hola"?
bool terminaConProgramming = texto.EndsWith("Programming"); // ¿Termina con "Programming"?

Console.WriteLine($"¿Contiene 'Hola'? {contieneHola}");
Console.WriteLine($"¿Empieza con 'Hola'? {empiezaConHola}");
Console.WriteLine($"¿Termina con 'Programming'? {terminaConProgramming}");

// Comparación
string texto2 = "HOLA MUNDO C# PROGRAMMING";
bool sonIguales = texto.Equals(texto2);               // Comparación exacta
bool sonIgualesIgnorandoMayusculas = texto.Equals(texto2, StringComparison.OrdinalIgnoreCase);

Console.WriteLine($"¿Son iguales? {sonIguales}");
Console.WriteLine($"¿Son iguales (ignorando mayúsculas)? {sonIgualesIgnorandoMayusculas}");
```

#### Explicación de los Métodos de Búsqueda:

**Línea 5: `texto.IndexOf('H')`**
- Busca la primera ocurrencia del carácter 'H'
- Retorna la posición (0) o -1 si no se encuentra

**Línea 6: `texto.IndexOf("Mundo")`**
- Busca la primera ocurrencia de la subcadena "Mundo"
- Retorna la posición (5) o -1 si no se encuentra

**Línea 7: `texto.LastIndexOf('o')`**
- Busca la última ocurrencia del carácter 'o'
- Retorna la posición (22) o -1 si no se encuentra

**Línea 12: `texto.Contains("Hola")`**
- Verifica si el string contiene la subcadena "Hola"
- Retorna `true` o `false`

**Línea 13: `texto.StartsWith("Hola")`**
- Verifica si el string empieza con "Hola"
- Retorna `true` o `false`

**Línea 14: `texto.EndsWith("Programming")`**
- Verifica si el string termina con "Programming"
- Retorna `true` o `false`

#### 4.3 Métodos de Transformación

```csharp
string texto = "  Hola Mundo C#  ";

Console.WriteLine($"Texto original: \"{texto}\"");

// Eliminación de espacios
string sinEspaciosInicio = texto.TrimStart();          // Elimina espacios al inicio
string sinEspaciosFinal = texto.TrimEnd();             // Elimina espacios al final
string sinEspacios = texto.Trim();                     // Elimina espacios al inicio y final

Console.WriteLine($"Sin espacios al inicio: \"{sinEspaciosInicio}\"");
Console.WriteLine($"Sin espacios al final: \"{sinEspaciosFinal}\"");
Console.WriteLine($"Sin espacios: \"{sinEspacios}\"");

// Cambio de mayúsculas y minúsculas
string mayusculas = texto.ToUpper();                   // Todo en mayúsculas
string minusculas = texto.ToLower();                   // Todo en minúsculas

Console.WriteLine($"En mayúsculas: \"{mayusculas}\"");
Console.WriteLine($"En minúsculas: \"{minusculas}\"");

// Reemplazo de caracteres
string reemplazado = texto.Replace("Mundo", "Universo"); // Reemplaza "Mundo" por "Universo"
string sinEspaciosReemplazados = texto.Replace(" ", ""); // Elimina todos los espacios

Console.WriteLine($"Reemplazado: \"{reemplazado}\"");
Console.WriteLine($"Sin espacios: \"{sinEspaciosReemplazados}\"");

// Subcadenas
string subcadena1 = texto.Substring(2, 4);            // Desde posición 2, 4 caracteres
string subcadena2 = texto.Substring(7);                // Desde posición 7 hasta el final

Console.WriteLine($"Subcadena 1: \"{subcadena1}\"");
Console.WriteLine($"Subcadena 2: \"{subcadena2}\"");
```

#### Explicación de los Métodos de Transformación:

**Línea 5: `texto.TrimStart()`**
- Elimina espacios en blanco al inicio del string
- Resultado: `"Hola Mundo C#  "`

**Línea 6: `texto.TrimEnd()`**
- Elimina espacios en blanco al final del string
- Resultado: `"  Hola Mundo C#"`

**Línea 7: `texto.Trim()`**
- Elimina espacios en blanco al inicio y final
- Resultado: `"Hola Mundo C#"`

**Línea 12: `texto.ToUpper()`**
- Convierte todos los caracteres a mayúsculas
- Resultado: `"  HOLA MUNDO C#  "`

**Línea 13: `texto.ToLower()`**
- Convierte todos los caracteres a minúsculas
- Resultado: `"  hola mundo c#  "`

**Línea 16: `texto.Replace("Mundo", "Universo")`**
- Reemplaza todas las ocurrencias de "Mundo" por "Universo"
- Resultado: `"  Hola Universo C#  "`

**Línea 20: `texto.Substring(2, 4)`**
- Extrae 4 caracteres desde la posición 2
- Resultado: `"Hola"`

### 5. StringBuilder para Manipulación Eficiente

StringBuilder es una clase que permite manipular strings de manera eficiente cuando se realizan muchas operaciones de concatenación.

#### 5.1 Uso Básico de StringBuilder

```csharp
using System.Text; // Necesario para usar StringBuilder

// Crear StringBuilder
StringBuilder sb = new StringBuilder();

// Agregar contenido
sb.Append("Hola");
sb.Append(" ");
sb.Append("Mundo");
sb.Append(" ");
sb.Append("C#");

// Convertir a string
string resultado = sb.ToString();

Console.WriteLine($"Resultado: \"{resultado}\"");
Console.WriteLine($"Longitud: {sb.Length}");
Console.WriteLine($"Capacidad: {sb.Capacity}");

// Limpiar StringBuilder
sb.Clear();
Console.WriteLine($"Después de limpiar - Longitud: {sb.Length}");
```

#### Explicación de StringBuilder:

**Línea 1: `using System.Text;`**
- Importa el namespace necesario para usar StringBuilder

**Línea 4: `StringBuilder sb = new StringBuilder();`**
- Crea una nueva instancia de StringBuilder vacía

**Línea 7: `sb.Append("Hola");`**
- Agrega "Hola" al final del StringBuilder
- No crea un nuevo objeto, modifica el existente

**Línea 8: `sb.Append(" ");`**
- Agrega un espacio al final

**Línea 9: `sb.Append("Mundo");`**
- Agrega "Mundo" al final

**Línea 10: `sb.Append(" ");`**
- Agrega otro espacio

**Línea 11: `sb.Append("C#");`**
- Agrega "C#" al final

**Línea 14: `string resultado = sb.ToString();`**
- Convierte el StringBuilder a un string normal
- Resultado: `"Hola Mundo C#"`

**Línea 20: `sb.Clear();`**
- Elimina todo el contenido del StringBuilder
- La longitud se convierte en 0

#### 5.2 StringBuilder con Métodos Avanzados

```csharp
StringBuilder sb = new StringBuilder("Inicio");

Console.WriteLine($"Contenido inicial: \"{sb}\"");

// Insertar en posición específica
sb.Insert(6, " del ");
Console.WriteLine($"Después de insertar: \"{sb}\"");

// Reemplazar contenido
sb.Replace("Inicio", "Comienzo");
Console.WriteLine($"Después de reemplazar: \"{sb}\"");

// Agregar con formato
sb.AppendFormat(" - {0} del {1}", "fin", "día");
Console.WriteLine($"Después de AppendFormat: \"{sb}\"");

// Agregar línea
sb.AppendLine();
sb.AppendLine("Nueva línea agregada");

// Mostrar resultado final
Console.WriteLine("Resultado final:");
Console.WriteLine(sb.ToString());

// Comparación de rendimiento
Console.WriteLine("\nComparación de rendimiento:");

// Con concatenación normal (ineficiente)
DateTime inicio1 = DateTime.Now;
string resultado1 = "";
for (int i = 0; i < 10000; i++)
{
    resultado1 += "número " + i + " ";
}
DateTime fin1 = DateTime.Now;
TimeSpan tiempo1 = fin1 - inicio1;

// Con StringBuilder (eficiente)
DateTime inicio2 = DateTime.Now;
StringBuilder sb2 = new StringBuilder();
for (int i = 0; i < 10000; i++)
{
    sb2.Append("número ").Append(i).Append(" ");
}
string resultado2 = sb2.ToString();
DateTime fin2 = DateTime.Now;
TimeSpan tiempo2 = fin2 - inicio2;

Console.WriteLine($"Tiempo con concatenación: {tiempo1.TotalMilliseconds:F2} ms");
Console.WriteLine($"Tiempo con StringBuilder: {tiempo2.TotalMilliseconds:F2} ms");
Console.WriteLine($"StringBuilder es {tiempo1.TotalMilliseconds / tiempo2.TotalMilliseconds:F1}x más rápido");
```

#### Explicación de Métodos Avanzados:

**Línea 4: `sb.Insert(6, " del ")`**
- Inserta " del " en la posición 6
- Resultado: `"Inicio del "`

**Línea 8: `sb.Replace("Inicio", "Comienzo")`**
- Reemplaza "Inicio" por "Comienzo"
- Resultado: `"Comienzo del "`

**Línea 12: `sb.AppendFormat(" - {0} del {1}", "fin", "día")`**
- Agrega texto con formato usando placeholders
- `{0}` se reemplaza con "fin", `{1}` se reemplaza con "día"
- Resultado: `"Comienzo del  - fin del día"`

**Línea 16: `sb.AppendLine()`**
- Agrega un salto de línea al final

**Línea 17: `sb.AppendLine("Nueva línea agregada")`**
- Agrega texto y luego un salto de línea

**Comparación de rendimiento:**
- La concatenación normal crea un nuevo string en cada iteración
- StringBuilder modifica el mismo objeto, siendo mucho más eficiente
- Para operaciones simples, la diferencia es mínima
- Para muchas operaciones, StringBuilder es significativamente más rápido

### 6. Formato de Strings

#### 6.1 Formatos Numéricos

```csharp
double numero = 123.456789;

Console.WriteLine("Formatos numéricos:");
Console.WriteLine($"Número original: {numero}");
Console.WriteLine($"2 decimales: {numero:F2}");
Console.WriteLine($"4 decimales: {numero:F4}");
Console.WriteLine($"Sin decimales: {numero:F0}");
Console.WriteLine($"Científico: {numero:E2}");
Console.WriteLine($"Porcentaje: {0.85:P1}");
Console.WriteLine($"Moneda: {numero:C2}");

// Formato con String.Format
string formato1 = String.Format("Número: {0:F2}", numero);
string formato2 = String.Format("Porcentaje: {0:P1}", 0.75);

Console.WriteLine($"\nCon String.Format:");
Console.WriteLine(formato1);
Console.WriteLine(formato2);
```

#### 6.2 Formatos de Fecha

```csharp
DateTime fecha = DateTime.Now;

Console.WriteLine("Formatos de fecha:");
Console.WriteLine($"Fecha completa: {fecha}");
Console.WriteLine($"Solo fecha: {fecha:d}");
Console.WriteLine($"Fecha larga: {fecha:D}");
Console.WriteLine($"Solo hora: {fecha:t}");
Console.WriteLine($"Hora larga: {fecha:T}");
Console.WriteLine($"Fecha y hora: {fecha:g}");
Console.WriteLine($"Fecha y hora larga: {fecha:G}");
Console.WriteLine($"Personalizado: {fecha:yyyy-MM-dd HH:mm:ss}");

// Formato personalizado
string fechaPersonalizada = fecha.ToString("dd/MM/yyyy HH:mm");
Console.WriteLine($"Formato personalizado: {fechaPersonalizada}");
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Manipulación de Strings
```csharp
string texto = "  Hola Mundo C# Programming  ";

Console.WriteLine($"Texto original: \"{texto}\"");
Console.WriteLine($"Longitud: {texto.Length}");

// Limpiar espacios
string limpio = texto.Trim();
Console.WriteLine($"Sin espacios: \"{limpio}\"");
Console.WriteLine($"Longitud limpia: {limpio.Length}");

// Cambiar a mayúsculas
string mayusculas = limpio.ToUpper();
Console.WriteLine($"En mayúsculas: \"{mayusculas}\"");

// Reemplazar palabras
string reemplazado = limpio.Replace("Mundo", "Universo");
Console.WriteLine($"Reemplazado: \"{reemplazado}\"");

// Verificar contenido
bool contieneHola = limpio.Contains("Hola");
bool empiezaConHola = limpio.StartsWith("Hola");
Console.WriteLine($"¿Contiene 'Hola'? {contieneHola}");
Console.WriteLine($"¿Empieza con 'Hola'? {empiezaConHola}");
```

### Ejercicio 2: Concatenación e Interpolación
```csharp
string nombre = "Ana";
string apellido = "Martínez";
int edad = 28;
string profesion = "Desarrolladora";

// Diferentes formas de concatenación
string presentacion1 = nombre + " " + apellido + " tiene " + edad + " años";
string presentacion2 = $"{nombre} {apellido} tiene {edad} años";
string presentacion3 = String.Format("{0} {1} tiene {2} años", nombre, apellido, edad);

Console.WriteLine("Presentaciones:");
Console.WriteLine($"1. {presentacion1}");
Console.WriteLine($"2. {presentacion2}");
Console.WriteLine($"3. {presentacion3}");

// Información completa
string infoCompleta = $"{nombre} {apellido} es {profesion} de {edad} años";
Console.WriteLine($"\nInformación completa: {infoCompleta}");

// Formato de edad
string edadFormateada = $"Edad: {edad:D2} años";
Console.WriteLine($"Edad formateada: {edadFormateada}");
```

### Ejercicio 3: StringBuilder
```csharp
using System.Text;

StringBuilder sb = new StringBuilder();

// Construir una lista
sb.AppendLine("Lista de tareas:");
sb.AppendLine("================");

string[] tareas = { "Estudiar C#", "Hacer ejercicios", "Leer documentación", "Practicar código" };

for (int i = 0; i < tareas.Length; i++)
{
    sb.AppendFormat("{0}. {1}\n", i + 1, tareas[i]);
}

sb.AppendLine("================");
sb.AppendLine("Total de tareas: " + tareas.Length);

// Mostrar resultado
Console.WriteLine(sb.ToString());

// Limpiar y reutilizar
sb.Clear();
sb.Append("StringBuilder reutilizado");
Console.WriteLine($"\nDespués de limpiar: \"{sb}\"");
```

### Ejercicio 4: Formato de Strings
```csharp
// Datos de un producto
string nombreProducto = "Laptop Gaming";
double precio = 1299.99;
int stock = 15;
DateTime fechaLanzamiento = new DateTime(2024, 1, 15);

Console.WriteLine("Información del Producto:");
Console.WriteLine("=========================");

// Formato de precio
string precioFormateado = precio.ToString("C2");
Console.WriteLine($"Nombre: {nombreProducto}");
Console.WriteLine($"Precio: {precioFormateado}");
Console.WriteLine($"Stock: {stock:D2} unidades");
Console.WriteLine($"Fecha de lanzamiento: {fechaLanzamiento:dd/MM/yyyy}");

// Formato de descuento
double descuento = 0.15; // 15%
double precioConDescuento = precio * (1 - descuento);
string descuentoFormateado = descuento.ToString("P0");
string precioFinalFormateado = precioConDescuento.ToString("C2");

Console.WriteLine($"\nDescuento: {descuentoFormateado}");
Console.WriteLine($"Precio final: {precioFinalFormateado}");
```

## 🔍 Conceptos Importantes a Recordar

1. **Los strings son inmutables** - no se pueden cambiar después de la creación
2. **La interpolación de strings** es más moderna y legible que la concatenación
3. **StringBuilder** es más eficiente para muchas operaciones de concatenación
4. **Los métodos de string** no modifican el original, crean uno nuevo
5. **IndexOf y LastIndexOf** son útiles para buscar contenido
6. **Contains, StartsWith y EndsWith** verifican el contenido del string
7. **Trim, ToUpper y ToLower** son métodos de transformación comunes
8. **El formato de strings** permite controlar la presentación de datos
9. **Los caracteres de escape** permiten incluir caracteres especiales

## ❓ Preguntas de Repaso

1. ¿Por qué son inmutables los strings en C#?
2. ¿Cuál es la diferencia entre `+` y `$` para concatenar strings?
3. ¿Cuándo deberías usar StringBuilder en lugar de concatenación normal?
4. ¿Qué hace el método `Trim()` en un string?
5. ¿Cómo formateas un número para mostrar solo 2 decimales?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Funciones Básicas**, donde veremos cómo crear y usar métodos en C#.

---

## 📚 Recursos Adicionales

- [Strings en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/strings/)
- [StringBuilder en C#](https://docs.microsoft.com/en-us/dotnet/api/system.text.stringbuilder)
- [Formato de strings](https://docs.microsoft.com/en-us/dotnet/standard/base-types/formatting-types)

---

**¡Excelente! Ahora dominas el manejo de strings y texto en C#! 🎯**
