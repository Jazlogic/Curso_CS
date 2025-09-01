# 🚀 Clase 5: LINQ to XML

## 📋 Información de la Clase

- **Módulo**: Mid Level 4 - LINQ y Expresiones Lambda
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Conocimientos de LINQ y XML básico

## 🎯 Objetivos de Aprendizaje

- Dominar LINQ to XML para consultar y manipular documentos XML
- Crear y modificar documentos XML usando LINQ
- Implementar transformaciones XML complejas
- Integrar XML con otros tipos de datos

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_expresiones_lambda_avanzadas.md) | Expresiones Lambda Avanzadas | |
| [Clase 2](clase_2_operadores_linq_basicos.md) | Operadores LINQ Básicos | |
| [Clase 3](clase_3_operadores_linq_avanzados.md) | Operadores LINQ Avanzados | |
| [Clase 4](clase_4_linq_to_objects.md) | LINQ to Objects | ← Anterior |
| **Clase 5** | **LINQ to XML** | ← Estás aquí |
| [Clase 6](clase_6_linq_to_sql.md) | LINQ to SQL | Siguiente → |
| [Clase 7](clase_7_linq_performance.md) | LINQ y Performance | |
| [Clase 8](clase_8_linq_optimization.md) | Optimización de LINQ | |
| [Clase 9](clase_9_linq_extension_methods.md) | Métodos de Extensión LINQ | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 4](../midLevel_4/README.md)**

---

## 📚 Contenido Teórico

### 1. LINQ to XML

LINQ to XML proporciona una forma moderna y eficiente de trabajar con XML usando la sintaxis de LINQ.

```csharp
// ===== LINQ TO XML - IMPLEMENTACIÓN COMPLETA =====
using System.Xml.Linq;

namespace LinqToXml
{
    // ===== CREACIÓN DE DOCUMENTOS XML =====
    namespace XmlCreation
    {
        public class XmlCreationExamples
        {
            // Crear documento XML básico
            public static XDocument CreateBasicXml()
            {
                return new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("Products",
                        new XElement("Product",
                            new XAttribute("Id", "1"),
                            new XElement("Name", "Laptop"),
                            new XElement("Price", "1200.00"),
                            new XElement("Category", "Electronics")
                        ),
                        new XElement("Product",
                            new XAttribute("Id", "2"),
                            new XElement("Name", "Mouse"),
                            new XElement("Price", "25.50"),
                            new XElement("Category", "Electronics")
                        )
                    )
                );
            }
            
            // Crear XML desde objetos
            public static XDocument CreateXmlFromObjects(IEnumerable<Product> products)
            {
                return new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("Products",
                        products.Select(p => new XElement("Product",
                            new XAttribute("Id", p.Id),
                            new XElement("Name", p.Name),
                            new XElement("Price", p.Price),
                            new XElement("Category", p.Category),
                            new XElement("Stock", p.Stock),
                            new XElement("IsActive", p.IsActive)
                        ))
                    )
                );
            }
            
            // Crear XML con estructura compleja
            public static XDocument CreateComplexXml(IEnumerable<Customer> customers, IEnumerable<Order> orders)
            {
                return new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("Store",
                        new XElement("Customers",
                            customers.Select(c => new XElement("Customer",
                                new XAttribute("Id", c.Id),
                                new XElement("Name", c.Name),
                                new XElement("Email", c.Email),
                                new XElement("Age", c.Age),
                                new XElement("City", c.City)
                            ))
                        ),
                        new XElement("Orders",
                            orders.Select(o => new XElement("Order",
                                new XAttribute("Id", o.Id),
                                new XAttribute("CustomerId", o.CustomerId),
                                new XElement("OrderDate", o.OrderDate.ToString("yyyy-MM-dd")),
                                new XElement("TotalAmount", o.TotalAmount),
                                new XElement("Status", o.Status),
                                new XElement("Items",
                                    o.Items.Select(i => new XElement("Item",
                                        new XAttribute("ProductId", i.ProductId),
                                        new XElement("Quantity", i.Quantity),
                                        new XElement("UnitPrice", i.UnitPrice),
                                        new XElement("TotalPrice", i.TotalPrice)
                                    ))
                                )
                            ))
                        )
                    )
                );
            }
        }
    }
    
    // ===== CONSULTAS XML BÁSICAS =====
    namespace BasicXmlQueries
    {
        public class BasicQueryExamples
        {
            // Consultar elementos por nombre
            public static IEnumerable<XElement> GetProductElements(XDocument doc)
            {
                return doc.Descendants("Product");
            }
            
            // Consultar elementos con atributos
            public static IEnumerable<XElement> GetProductsById(XDocument doc, int id)
            {
                return doc.Descendants("Product")
                    .Where(p => (int)p.Attribute("Id") == id);
            }
            
            // Consultar valores de elementos
            public static IEnumerable<string> GetProductNames(XDocument doc)
            {
                return doc.Descendants("Product")
                    .Elements("Name")
                    .Select(n => n.Value);
            }
            
            // Consultar con filtros
            public static IEnumerable<XElement> GetExpensiveProducts(XDocument doc, decimal minPrice)
            {
                return doc.Descendants("Product")
                    .Where(p => (decimal)p.Element("Price") >= minPrice);
            }
            
            // Consultar con múltiples condiciones
            public static IEnumerable<XElement> GetActiveProductsInCategory(XDocument doc, string category)
            {
                return doc.Descendants("Product")
                    .Where(p => p.Element("Category").Value == category &&
                               (bool)p.Element("IsActive"));
            }
        }
    }
    
    // ===== CONSULTAS XML AVANZADAS =====
    namespace AdvancedXmlQueries
    {
        public class AdvancedQueryExamples
        {
            // Consulta con join implícito
            public static IEnumerable<object> GetCustomerOrders(XDocument doc)
            {
                var customers = doc.Descendants("Customer");
                var orders = doc.Descendants("Order");
                
                return from customer in customers
                       join order in orders on (int)customer.Attribute("Id") equals (int)order.Attribute("CustomerId")
                       select new
                       {
                           CustomerName = customer.Element("Name").Value,
                           CustomerEmail = customer.Element("Email").Value,
                           OrderId = (int)order.Attribute("Id"),
                           OrderDate = DateTime.Parse(order.Element("OrderDate").Value),
                           TotalAmount = (decimal)order.Element("TotalAmount")
                       };
            }
            
            // Consulta con agregación
            public static IEnumerable<object> GetCategoryStats(XDocument doc)
            {
                return doc.Descendants("Product")
                    .GroupBy(p => p.Element("Category").Value, (category, products) => new
                    {
                        Category = category,
                        ProductCount = products.Count(),
                        AveragePrice = products.Average(p => (decimal)p.Element("Price")),
                        TotalStock = products.Sum(p => (int)p.Element("Stock"))
                    });
            }
            
            // Consulta con subconsultas
            public static IEnumerable<object> GetCustomerOrderSummary(XDocument doc)
            {
                return doc.Descendants("Customer")
                    .Select(customer => new
                    {
                        CustomerId = (int)customer.Attribute("Id"),
                        CustomerName = customer.Element("Name").Value,
                        OrderCount = doc.Descendants("Order")
                            .Count(o => (int)o.Attribute("CustomerId") == (int)customer.Attribute("Id")),
                        TotalSpent = doc.Descendants("Order")
                            .Where(o => (int)o.Attribute("CustomerId") == (int)customer.Attribute("Id"))
                            .Sum(o => (decimal)o.Element("TotalAmount"))
                    });
            }
            
            // Consulta con ordenamiento
            public static IEnumerable<XElement> GetProductsOrderedByPrice(XDocument doc)
            {
                return doc.Descendants("Product")
                    .OrderBy(p => (decimal)p.Element("Price"));
            }
            
            // Consulta con paginación
            public static IEnumerable<XElement> GetProductsPage(XDocument doc, int pageNumber, int pageSize)
            {
                return doc.Descendants("Product")
                    .OrderBy(p => p.Element("Name").Value)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);
            }
        }
    }
    
    // ===== MANIPULACIÓN DE XML =====
    namespace XmlManipulation
    {
        public class ManipulationExamples
        {
            // Agregar elemento
            public static void AddProduct(XDocument doc, Product product)
            {
                var productsElement = doc.Element("Products");
                productsElement.Add(new XElement("Product",
                    new XAttribute("Id", product.Id),
                    new XElement("Name", product.Name),
                    new XElement("Price", product.Price),
                    new XElement("Category", product.Category),
                    new XElement("Stock", product.Stock),
                    new XElement("IsActive", product.IsActive)
                ));
            }
            
            // Actualizar elemento
            public static void UpdateProduct(XDocument doc, int productId, decimal newPrice)
            {
                var product = doc.Descendants("Product")
                    .FirstOrDefault(p => (int)p.Attribute("Id") == productId);
                
                if (product != null)
                {
                    product.Element("Price").Value = newPrice.ToString();
                }
            }
            
            // Eliminar elemento
            public static void RemoveProduct(XDocument doc, int productId)
            {
                var product = doc.Descendants("Product")
                    .FirstOrDefault(p => (int)p.Attribute("Id") == productId);
                
                product?.Remove();
            }
            
            // Agregar atributo
            public static void AddAttribute(XDocument doc, int productId, string attributeName, string attributeValue)
            {
                var product = doc.Descendants("Product")
                    .FirstOrDefault(p => (int)p.Attribute("Id") == productId);
                
                if (product != null)
                {
                    product.Add(new XAttribute(attributeName, attributeValue));
                }
            }
            
            // Modificar múltiples elementos
            public static void UpdateProductPrices(XDocument doc, string category, decimal discountPercent)
            {
                var products = doc.Descendants("Product")
                    .Where(p => p.Element("Category").Value == category);
                
                foreach (var product in products)
                {
                    var priceElement = product.Element("Price");
                    var currentPrice = (decimal)priceElement;
                    var newPrice = currentPrice * (1 - discountPercent / 100);
                    priceElement.Value = newPrice.ToString();
                }
            }
        }
    }
    
    // ===== TRANSFORMACIONES XML =====
    namespace XmlTransformations
    {
        public class TransformationExamples
        {
            // Transformar XML a objetos
            public static IEnumerable<Product> XmlToProducts(XDocument doc)
            {
                return doc.Descendants("Product")
                    .Select(p => new Product(
                        (int)p.Attribute("Id"),
                        p.Element("Name").Value,
                        (decimal)p.Element("Price"),
                        p.Element("Category").Value,
                        (int)p.Element("Stock")
                    ));
            }
            
            // Transformar objetos a XML
            public static XDocument ProductsToXml(IEnumerable<Product> products)
            {
                return new XDocument(
                    new XElement("Products",
                        products.Select(p => new XElement("Product",
                            new XAttribute("Id", p.Id),
                            new XElement("Name", p.Name),
                            new XElement("Price", p.Price),
                            new XElement("Category", p.Category),
                            new XElement("Stock", p.Stock)
                        ))
                    )
                );
            }
            
            // Transformar XML a otro formato XML
            public static XDocument TransformToSummary(XDocument doc)
            {
                return new XDocument(
                    new XElement("ProductSummary",
                        doc.Descendants("Product")
                            .GroupBy(p => p.Element("Category").Value, (category, products) => new XElement("Category",
                                new XAttribute("Name", category),
                                new XElement("ProductCount", products.Count()),
                                new XElement("AveragePrice", products.Average(p => (decimal)p.Element("Price"))),
                                new XElement("TotalStock", products.Sum(p => (int)p.Element("Stock")))
                            ))
                    )
                );
            }
            
            // Transformar XML a HTML
            public static XDocument TransformToHtml(XDocument doc)
            {
                return new XDocument(
                    new XElement("html",
                        new XElement("head",
                            new XElement("title", "Product Catalog")
                        ),
                        new XElement("body",
                            new XElement("h1", "Product Catalog"),
                            new XElement("table",
                                new XElement("tr",
                                    new XElement("th", "ID"),
                                    new XElement("th", "Name"),
                                    new XElement("th", "Price"),
                                    new XElement("th", "Category")
                                ),
                                doc.Descendants("Product")
                                    .Select(p => new XElement("tr",
                                        new XElement("td", p.Attribute("Id").Value),
                                        new XElement("td", p.Element("Name").Value),
                                        new XElement("td", p.Element("Price").Value),
                                        new XElement("td", p.Element("Category").Value)
                                    ))
                            )
                        )
                    )
                );
            }
        }
    }
    
    // ===== VALIDACIÓN XML =====
    namespace XmlValidation
    {
        public class ValidationExamples
        {
            // Validar estructura XML
            public static bool ValidateProductStructure(XDocument doc)
            {
                return doc.Descendants("Product")
                    .All(p => p.Attribute("Id") != null &&
                              p.Element("Name") != null &&
                              p.Element("Price") != null &&
                              p.Element("Category") != null);
            }
            
            // Validar tipos de datos
            public static bool ValidateProductData(XDocument doc)
            {
                return doc.Descendants("Product")
                    .All(p => int.TryParse(p.Attribute("Id")?.Value, out _) &&
                              decimal.TryParse(p.Element("Price")?.Value, out _) &&
                              int.TryParse(p.Element("Stock")?.Value, out _));
            }
            
            // Validar reglas de negocio
            public static bool ValidateBusinessRules(XDocument doc)
            {
                return doc.Descendants("Product")
                    .All(p => (decimal)p.Element("Price") > 0 &&
                              (int)p.Element("Stock") >= 0 &&
                              !string.IsNullOrEmpty(p.Element("Name").Value));
            }
            
            // Obtener elementos inválidos
            public static IEnumerable<XElement> GetInvalidProducts(XDocument doc)
            {
                return doc.Descendants("Product")
                    .Where(p => (decimal)p.Element("Price") <= 0 ||
                               (int)p.Element("Stock") < 0 ||
                               string.IsNullOrEmpty(p.Element("Name").Value));
            }
        }
    }
    
    // ===== LECTURA Y ESCRITURA DE ARCHIVOS XML =====
    namespace XmlFileOperations
    {
        public class FileOperationExamples
        {
            // Guardar XML a archivo
            public static void SaveXmlToFile(XDocument doc, string filePath)
            {
                doc.Save(filePath);
            }
            
            // Cargar XML desde archivo
            public static XDocument LoadXmlFromFile(string filePath)
            {
                return XDocument.Load(filePath);
            }
            
            // Guardar XML con formato
            public static void SaveXmlFormatted(XDocument doc, string filePath)
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\n"
                };
                
                using (var writer = XmlWriter.Create(filePath, settings))
                {
                    doc.Save(writer);
                }
            }
            
            // Cargar XML con validación
            public static XDocument LoadXmlWithValidation(string filePath)
            {
                try
                {
                    return XDocument.Load(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading XML: {ex.Message}");
                    return new XDocument();
                }
            }
            
            // Crear XML desde string
            public static XDocument ParseXmlString(string xmlString)
            {
                return XDocument.Parse(xmlString);
            }
            
            // Convertir XML a string
            public static string XmlToString(XDocument doc)
            {
                return doc.ToString();
            }
        }
    }
}

// Uso de LINQ to XML
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== LINQ to XML - Clase 5 ===\n");
        
        // Crear datos de ejemplo
        var products = new List<Product>
        {
            new Product(1, "Laptop", 1200.00m, "Electronics", 15),
            new Product(2, "Mouse", 25.50m, "Electronics", 50),
            new Product(3, "Keyboard", 75.00m, "Electronics", 30),
            new Product(4, "Book", 15.99m, "Books", 100),
            new Product(5, "Pen", 2.50m, "Office", 200)
        };
        
        var customers = new List<Customer>
        {
            new Customer(1, "John Doe", "john@example.com", 30, "New York"),
            new Customer(2, "Jane Smith", "jane@example.com", 25, "Los Angeles")
        };
        
        var orders = new List<Order>
        {
            new Order(1, 1),
            new Order(2, 2)
        };
        
        orders[0].AddItem(new OrderItem(1, 1, 1200.00m));
        orders[1].AddItem(new OrderItem(2, 2, 25.50m));
        
        // Ejemplos de creación de XML
        Console.WriteLine("1. Creación de XML:");
        var xmlDoc = XmlCreation.XmlCreationExamples.CreateXmlFromObjects(products);
        Console.WriteLine("XML creado exitosamente");
        
        // Ejemplos de consultas básicas
        Console.WriteLine("\n2. Consultas Básicas:");
        var productNames = BasicXmlQueries.BasicQueryExamples.GetProductNames(xmlDoc);
        Console.WriteLine($"Nombres de productos: {string.Join(", ", productNames)}");
        
        // Ejemplos de consultas avanzadas
        Console.WriteLine("\n3. Consultas Avanzadas:");
        var categoryStats = AdvancedXmlQueries.AdvancedQueryExamples.GetCategoryStats(xmlDoc);
        foreach (var stat in categoryStats)
        {
            Console.WriteLine($"Categoría: {stat.Category}, Productos: {stat.ProductCount}, Precio Promedio: {stat.AveragePrice:C}");
        }
        
        // Ejemplos de manipulación
        Console.WriteLine("\n4. Manipulación de XML:");
        XmlManipulation.ManipulationExamples.AddProduct(xmlDoc, new Product(6, "Monitor", 300.00m, "Electronics", 10));
        Console.WriteLine("Producto agregado al XML");
        
        // Ejemplos de transformaciones
        Console.WriteLine("\n5. Transformaciones:");
        var productsFromXml = XmlTransformations.TransformationExamples.XmlToProducts(xmlDoc);
        Console.WriteLine($"Productos transformados: {productsFromXml.Count()}");
        
        // Ejemplos de validación
        Console.WriteLine("\n6. Validación:");
        var isValid = XmlValidation.ValidationExamples.ValidateProductStructure(xmlDoc);
        Console.WriteLine($"Estructura XML válida: {isValid}");
        
        // Ejemplos de operaciones de archivo
        Console.WriteLine("\n7. Operaciones de Archivo:");
        var xmlString = XmlFileOperations.FileOperationExamples.XmlToString(xmlDoc);
        Console.WriteLine($"XML convertido a string: {xmlString.Length} caracteres");
        
        Console.WriteLine("\n✅ LINQ to XML funcionando correctamente!");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Creación y Consulta XML
Crea documentos XML y consulta elementos usando LINQ to XML.

### Ejercicio 2: Manipulación XML
Implementa operaciones de agregar, actualizar y eliminar elementos XML.

### Ejercicio 3: Transformaciones XML
Crea transformaciones entre XML y otros formatos de datos.

## 🔍 Puntos Clave

1. **Creación de XML** usando XDocument y XElement
2. **Consultas básicas** con Descendants, Elements y Attributes
3. **Consultas avanzadas** con joins, agregaciones y subconsultas
4. **Manipulación XML** agregando, actualizando y eliminando elementos
5. **Transformaciones** entre XML y objetos
6. **Validación XML** de estructura y reglas de negocio
7. **Operaciones de archivo** para leer y escribir XML
8. **Integración** con otros tipos de datos

## 📚 Recursos Adicionales

- [Microsoft Docs - LINQ to XML](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/linq-to-xml)
- [XDocument Class](https://docs.microsoft.com/en-us/dotnet/api/system.xml.linq.xdocument)

---

**🎯 ¡Has completado la Clase 5! Ahora comprendes LINQ to XML**

**📚 [Siguiente: Clase 6 - LINQ to SQL](clase_6_linq_to_sql.md)**
