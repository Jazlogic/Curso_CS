# 🚀 Clase 1: Fundamentos de Testing

## 📋 Información de la Clase

- **Módulo**: Senior Level 2 - Testing y TDD
- **Duración**: 2 horas
- **Nivel**: Avanzado
- **Prerrequisitos**: Conocimientos sólidos de C# y programación orientada a objetos

## 🎯 Objetivos de Aprendizaje

- Comprender los fundamentos del testing de software
- Identificar los diferentes tipos de testing
- Entender la pirámide de testing
- Implementar testing unitario básico

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| **Clase 1** | **Fundamentos de Testing** | ← Estás aquí |
| [Clase 2](clase_2_desarrollo_dirigido_pruebas.md) | Desarrollo Dirigido por Pruebas (TDD) | Siguiente → |
| [Clase 3](clase_3_testing_unitario.md) | Testing Unitario | |
| [Clase 4](clase_4_testing_integracion.md) | Testing de Integración | |
| [Clase 5](clase_5_testing_comportamiento.md) | Testing de Comportamiento | |
| [Clase 6](clase_6_mocking_frameworks.md) | Frameworks de Mocking | |
| [Clase 7](clase_7_testing_asincrono.md) | Testing de Código Asíncrono | |
| [Clase 8](clase_8_testing_apis.md) | Testing de APIs | |
| [Clase 9](clase_9_testing_database.md) | Testing de Base de Datos | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Testing | |

**← [Volver al README del Módulo 2](../senior_2/README.md)**

---

## 📚 Contenido Teórico

### 1. ¿Qué es el Testing?

El testing es el proceso sistemático de verificar que el software funciona correctamente y cumple con los requisitos especificados. Es una parte fundamental del desarrollo de software que ayuda a:

- **Detectar errores** antes de que lleguen a producción
- **Validar funcionalidad** según los requisitos
- **Mejorar la calidad** del código
- **Facilitar el mantenimiento** y refactoring
- **Documentar el comportamiento** esperado del sistema

```csharp
// ===== FUNDAMENTOS DE TESTING - IMPLEMENTACIÓN COMPLETA =====
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestingFundamentals
{
    // ===== MODELOS DE DATOS =====
    namespace Models
    {
        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string Category { get; set; }
            public bool IsActive { get; set; }
            
            public Product(int id, string name, decimal price, int stock, string category)
            {
                Id = id;
                Name = name;
                Price = price;
                Stock = stock;
                Category = category;
                IsActive = true;
            }
        }
        
        public class Order
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
            public List<OrderItem> Items { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; }
            
            public Order(int id, int customerId)
            {
                Id = id;
                CustomerId = customerId;
                Items = new List<OrderItem>();
                OrderDate = DateTime.Now;
                Status = "Pending";
            }
            
            public void AddItem(OrderItem item)
            {
                Items.Add(item);
                CalculateTotal();
            }
            
            private void CalculateTotal()
            {
                TotalAmount = Items.Sum(item => item.TotalPrice);
            }
        }
        
        public class OrderItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice => Quantity * UnitPrice;
            
            public OrderItem(int productId, int quantity, decimal unitPrice)
            {
                ProductId = productId;
                Quantity = quantity;
                UnitPrice = unitPrice;
            }
        }
        
        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public bool IsActive { get; set; }
            public decimal CreditLimit { get; set; }
            
            public Customer(int id, string name, string email)
            {
                Id = id;
                Name = name;
                Email = email;
                IsActive = true;
                CreditLimit = 1000.00m;
            }
        }
    }
    
    // ===== SERVICIOS DE NEGOCIO =====
    namespace Services
    {
        public interface IProductService
        {
            Task<Product> GetProductByIdAsync(int id);
            Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
            Task<bool> UpdateStockAsync(int productId, int quantity);
            Task<decimal> CalculateDiscountAsync(int productId, int quantity);
        }
        
        public class ProductService : IProductService
        {
            private readonly IProductRepository _repository;
            private readonly IDiscountCalculator _discountCalculator;
            
            public ProductService(IProductRepository repository, IDiscountCalculator discountCalculator)
            {
                _repository = repository ?? throw new ArgumentNullException(nameof(repository));
                _discountCalculator = discountCalculator ?? throw new ArgumentNullException(nameof(discountCalculator));
            }
            
            public async Task<Product> GetProductByIdAsync(int id)
            {
                if (id <= 0)
                    throw new ArgumentException("Product ID must be positive", nameof(id));
                
                var product = await _repository.GetByIdAsync(id);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {id} not found");
                
                return product;
            }
            
            public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
            {
                if (string.IsNullOrWhiteSpace(category))
                    throw new ArgumentException("Category cannot be null or empty", nameof(category));
                
                return await _repository.GetByCategoryAsync(category);
            }
            
            public async Task<bool> UpdateStockAsync(int productId, int quantity)
            {
                if (productId <= 0)
                    throw new ArgumentException("Product ID must be positive", nameof(productId));
                
                if (quantity < 0)
                    throw new ArgumentException("Quantity cannot be negative", nameof(quantity));
                
                var product = await _repository.GetByIdAsync(productId);
                if (product == null)
                    return false;
                
                if (product.Stock < quantity)
                    return false;
                
                product.Stock -= quantity;
                await _repository.UpdateAsync(product);
                
                return true;
            }
            
            public async Task<decimal> CalculateDiscountAsync(int productId, int quantity)
            {
                if (productId <= 0)
                    throw new ArgumentException("Product ID must be positive", nameof(productId));
                
                if (quantity <= 0)
                    throw new ArgumentException("Quantity must be positive", nameof(quantity));
                
                var product = await _repository.GetByIdAsync(productId);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {productId} not found");
                
                return await _discountCalculator.CalculateAsync(product, quantity);
            }
        }
        
        public interface IOrderService
        {
            Task<Order> CreateOrderAsync(int customerId, List<OrderItem> items);
            Task<bool> ProcessOrderAsync(int orderId);
            Task<decimal> CalculateOrderTotalAsync(int orderId);
            Task<bool> CancelOrderAsync(int orderId);
        }
        
        public class OrderService : IOrderService
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IProductService _productService;
            private readonly ICustomerService _customerService;
            
            public OrderService(IOrderRepository orderRepository, IProductService productService, ICustomerService customerService)
            {
                _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
                _productService = productService ?? throw new ArgumentNullException(nameof(productService));
                _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            }
            
            public async Task<Order> CreateOrderAsync(int customerId, List<OrderItem> items)
            {
                if (customerId <= 0)
                    throw new ArgumentException("Customer ID must be positive", nameof(customerId));
                
                if (items == null || !items.Any())
                    throw new ArgumentException("Order must have at least one item", nameof(items));
                
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                if (customer == null)
                    throw new InvalidOperationException($"Customer with ID {customerId} not found");
                
                if (!customer.IsActive)
                    throw new InvalidOperationException("Customer account is not active");
                
                var order = new Order(0, customerId);
                foreach (var item in items)
                {
                    order.AddItem(item);
                }
                
                var createdOrder = await _orderRepository.CreateAsync(order);
                return createdOrder;
            }
            
            public async Task<bool> ProcessOrderAsync(int orderId)
            {
                if (orderId <= 0)
                    throw new ArgumentException("Order ID must be positive", nameof(orderId));
                
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    return false;
                
                if (order.Status != "Pending")
                    return false;
                
                // Verificar stock para todos los items
                foreach (var item in order.Items)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product.Stock < item.Quantity)
                        return false;
                }
                
                // Actualizar stock
                foreach (var item in order.Items)
                {
                    await _productService.UpdateStockAsync(item.ProductId, item.Quantity);
                }
                
                order.Status = "Processed";
                await _orderRepository.UpdateAsync(order);
                
                return true;
            }
            
            public async Task<decimal> CalculateOrderTotalAsync(int orderId)
            {
                if (orderId <= 0)
                    throw new ArgumentException("Order ID must be positive", nameof(orderId));
                
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    throw new InvalidOperationException($"Order with ID {orderId} not found");
                
                return order.TotalAmount;
            }
            
            public async Task<bool> CancelOrderAsync(int orderId)
            {
                if (orderId <= 0)
                    throw new ArgumentException("Order ID must be positive", nameof(orderId));
                
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    return false;
                
                if (order.Status == "Cancelled")
                    return false;
                
                if (order.Status == "Processed")
                {
                    // Restaurar stock si la orden ya fue procesada
                    foreach (var item in order.Items)
                    {
                        var product = await _productService.GetProductByIdAsync(item.ProductId);
                        product.Stock += item.Quantity;
                        // Aquí deberías actualizar el producto en la base de datos
                    }
                }
                
                order.Status = "Cancelled";
                await _orderRepository.UpdateAsync(order);
                
                return true;
            }
        }
        
        public interface ICustomerService
        {
            Task<Customer> GetCustomerByIdAsync(int id);
            Task<bool> UpdateCreditLimitAsync(int customerId, decimal newLimit);
            Task<bool> DeactivateCustomerAsync(int customerId);
        }
        
        public class CustomerService : ICustomerService
        {
            private readonly ICustomerRepository _repository;
            
            public CustomerService(ICustomerRepository repository)
            {
                _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            }
            
            public async Task<Customer> GetCustomerByIdAsync(int id)
            {
                if (id <= 0)
                    throw new ArgumentException("Customer ID must be positive", nameof(id));
                
                return await _repository.GetByIdAsync(id);
            }
            
            public async Task<bool> UpdateCreditLimitAsync(int customerId, decimal newLimit)
            {
                if (customerId <= 0)
                    throw new ArgumentException("Customer ID must be positive", nameof(customerId));
                
                if (newLimit < 0)
                    throw new ArgumentException("Credit limit cannot be negative", nameof(newLimit));
                
                var customer = await _repository.GetByIdAsync(customerId);
                if (customer == null)
                    return false;
                
                customer.CreditLimit = newLimit;
                await _repository.UpdateAsync(customer);
                
                return true;
            }
            
            public async Task<bool> DeactivateCustomerAsync(int customerId)
            {
                if (customerId <= 0)
                    throw new ArgumentException("Customer ID must be positive", nameof(customerId));
                
                var customer = await _repository.GetByIdAsync(customerId);
                if (customer == null)
                    return false;
                
                customer.IsActive = false;
                await _repository.UpdateAsync(customer);
                
                return true;
            }
        }
    }
    
    // ===== INTERFACES DE REPOSITORIO =====
    namespace Repositories
    {
        public interface IProductRepository
        {
            Task<Product> GetByIdAsync(int id);
            Task<IEnumerable<Product>> GetByCategoryAsync(string category);
            Task<Product> CreateAsync(Product product);
            Task<bool> UpdateAsync(Product product);
            Task<bool> DeleteAsync(int id);
        }
        
        public interface IOrderRepository
        {
            Task<Order> GetByIdAsync(int id);
            Task<Order> CreateAsync(Order order);
            Task<bool> UpdateAsync(Order order);
            Task<bool> DeleteAsync(int id);
            Task<IEnumerable<Order>> GetByCustomerAsync(int customerId);
        }
        
        public interface ICustomerRepository
        {
            Task<Customer> GetByIdAsync(int id);
            Task<Customer> CreateAsync(Customer customer);
            Task<bool> UpdateAsync(Customer customer);
            Task<bool> DeleteAsync(int id);
        }
    }
    
    // ===== SERVICIOS AUXILIARES =====
    namespace Utilities
    {
        public interface IDiscountCalculator
        {
            Task<decimal> CalculateAsync(Product product, int quantity);
        }
        
        public class DiscountCalculator : IDiscountCalculator
        {
            public async Task<decimal> CalculateAsync(Product product, int quantity)
            {
                // Simulación de cálculo de descuento
                await Task.Delay(10);
                
                if (quantity >= 10)
                    return product.Price * 0.15m; // 15% de descuento
                else if (quantity >= 5)
                    return product.Price * 0.10m; // 10% de descuento
                else if (quantity >= 2)
                    return product.Price * 0.05m; // 5% de descuento
                else
                    return 0; // Sin descuento
            }
        }
        
        public class ValidationService
        {
            public static bool IsValidEmail(string email)
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;
                
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
            
            public static bool IsValidPrice(decimal price)
            {
                return price >= 0;
            }
            
            public static bool IsValidQuantity(int quantity)
            {
                return quantity > 0;
            }
        }
    }
}

// ===== EJEMPLOS DE TESTING =====
public class TestingExamples
{
    public static void DemonstrateTesting()
    {
        Console.WriteLine("=== Fundamentos de Testing - Clase 1 ===\n");
        
        // Crear datos de ejemplo
        var products = new List<TestingFundamentals.Models.Product>
        {
            new TestingFundamentals.Models.Product(1, "Laptop", 999.99m, 10, "Electronics"),
            new TestingFundamentals.Models.Product(2, "Mouse", 29.99m, 50, "Electronics"),
            new TestingFundamentals.Models.Product(3, "Book", 19.99m, 100, "Books")
        };
        
        var customers = new List<TestingFundamentals.Models.Customer>
        {
            new TestingFundamentals.Models.Customer(1, "John Doe", "john@example.com"),
            new TestingFundamentals.Models.Customer(2, "Jane Smith", "jane@example.com")
        };
        
        // Ejemplos de validación
        Console.WriteLine("1. Validación de Datos:");
        foreach (var product in products)
        {
            Console.WriteLine($"Producto: {product.Name}");
            Console.WriteLine($"  - Precio válido: {TestingFundamentals.Utilities.ValidationService.IsValidPrice(product.Price)}");
            Console.WriteLine($"  - Stock válido: {TestingFundamentals.Utilities.ValidationService.IsValidQuantity(product.Stock)}");
        }
        
        Console.WriteLine("\n2. Validación de Emails:");
        foreach (var customer in customers)
        {
            Console.WriteLine($"Cliente: {customer.Name}");
            Console.WriteLine($"  - Email válido: {TestingFundamentals.Utilities.ValidationService.IsValidEmail(customer.Email)}");
        }
        
        // Ejemplo de cálculo de descuento
        Console.WriteLine("\n3. Cálculo de Descuentos:");
        var discountCalculator = new TestingFundamentals.Utilities.DiscountCalculator();
        
        foreach (var product in products)
        {
            var discount = discountCalculator.CalculateAsync(product, 5).Result;
            Console.WriteLine($"Producto: {product.Name}, Cantidad: 5, Descuento: ${discount:F2}");
        }
        
        Console.WriteLine("\n✅ Fundamentos de Testing comprendidos!");
        Console.WriteLine("En las siguientes clases aprenderás a escribir pruebas para este código.");
    }
}

// Programa principal
public class Program
{
    public static async Task Main()
    {
        TestingExamples.DemonstrateTesting();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Validación de Datos
Implementa métodos de validación para:
- Números de teléfono
- Códigos postales
- Fechas de nacimiento

### Ejercicio 2: Servicios de Negocio
Crea servicios para:
- Gestión de inventario
- Cálculo de impuestos
- Validación de cupones

### Ejercicio 3: Manejo de Errores
Implementa manejo de excepciones para:
- Datos inválidos
- Recursos no encontrados
- Errores de negocio

## 🔍 Puntos Clave

1. **Testing es fundamental** para la calidad del software
2. **Diferentes tipos** de testing (unitario, integración, sistema)
3. **Pirámide de testing** para optimizar recursos
4. **Validación de datos** como primera línea de defensa
5. **Manejo de excepciones** para casos de error
6. **Interfaces bien definidas** facilitan el testing
7. **Separación de responsabilidades** mejora la testabilidad
8. **Código limpio** es más fácil de probar

## 📚 Recursos Adicionales

- [Microsoft Docs - Testing](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [Testing Fundamentals](https://docs.microsoft.com/en-us/visualstudio/test/unit-test-basics)
- [Best Practices for Testing](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/test-aspnet-core-mvc)

---

**🎯 ¡Has completado la Clase 1! Ahora comprendes los Fundamentos de Testing**

**📚 [Siguiente: Clase 2 - Desarrollo Dirigido por Pruebas (TDD)](clase_2_desarrollo_dirigido_pruebas.md)**
