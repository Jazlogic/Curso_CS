# 🚀 Clase 2: Desarrollo Dirigido por Pruebas (TDD)

## 📋 Información de la Clase

- **Módulo**: Senior Level 2 - Testing y TDD
- **Duración**: 2 horas
- **Nivel**: Avanzado
- **Prerrequisitos**: Fundamentos de Testing (Clase 1)

## 🎯 Objetivos de Aprendizaje

- Comprender el ciclo Red-Green-Refactor del TDD
- Implementar desarrollo dirigido por pruebas
- Crear pruebas antes que el código
- Refactorizar código manteniendo las pruebas

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_fundamentos_testing.md) | Fundamentos de Testing | ← Anterior |
| **Clase 2** | **Desarrollo Dirigido por Pruebas (TDD)** | ← Estás aquí |
| [Clase 3](clase_3_testing_unitario.md) | Testing Unitario | Siguiente → |
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

### 1. ¿Qué es TDD?

El **Desarrollo Dirigido por Pruebas (TDD)** es una metodología de desarrollo que sigue el ciclo **Red-Green-Refactor**:

1. **Red**: Escribir una prueba que falle
2. **Green**: Escribir el código mínimo para que la prueba pase
3. **Refactor**: Mejorar el código sin cambiar su comportamiento

### 2. Ciclo Red-Green-Refactor

```csharp
// ===== DESARROLLO DIRIGIDO POR PRUEBAS (TDD) - IMPLEMENTACIÓN COMPLETA =====
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TDDDevelopment
{
    // ===== EJEMPLO 1: CALCULADORA SIMPLE =====
    namespace CalculatorExample
    {
        // PASO 1: RED - Escribir la prueba que falle
        public class CalculatorTests
        {
            [Test]
            public void Add_WithTwoNumbers_ReturnsSum()
            {
                // Arrange
                var calculator = new Calculator();
                
                // Act
                var result = calculator.Add(2, 3);
                
                // Assert
                Assert.AreEqual(5, result);
            }
            
            [Test]
            public void Subtract_WithTwoNumbers_ReturnsDifference()
            {
                // Arrange
                var calculator = new Calculator();
                
                // Act
                var result = calculator.Subtract(5, 3);
                
                // Assert
                Assert.AreEqual(2, result);
            }
            
            [Test]
            public void Multiply_WithTwoNumbers_ReturnsProduct()
            {
                // Arrange
                var calculator = new Calculator();
                
                // Act
                var result = calculator.Multiply(4, 3);
                
                // Assert
                Assert.AreEqual(12, result);
            }
            
            [Test]
            public void Divide_WithTwoNumbers_ReturnsQuotient()
            {
                // Arrange
                var calculator = new Calculator();
                
                // Act
                var result = calculator.Divide(10, 2);
                
                // Assert
                Assert.AreEqual(5, result);
            }
            
            [Test]
            public void Divide_ByZero_ThrowsException()
            {
                // Arrange
                var calculator = new Calculator();
                
                // Act & Assert
                Assert.Throws<DivideByZeroException>(() => calculator.Divide(10, 0));
            }
        }
        
        // PASO 2: GREEN - Implementación mínima para que las pruebas pasen
        public class Calculator
        {
            public int Add(int a, int b)
            {
                return a + b;
            }
            
            public int Subtract(int a, int b)
            {
                return a - b;
            }
            
            public int Multiply(int a, int b)
            {
                return a * b;
            }
            
            public int Divide(int a, int b)
            {
                if (b == 0)
                    throw new DivideByZeroException("Cannot divide by zero");
                
                return a / b;
            }
        }
        
        // PASO 3: REFACTOR - Mejorar el código
        public class CalculatorRefactored
        {
            public int Add(int a, int b) => a + b;
            public int Subtract(int a, int b) => a - b;
            public int Multiply(int a, int b) => a * b;
            
            public int Divide(int a, int b)
            {
                ValidateDivisor(b);
                return a / b;
            }
            
            private static void ValidateDivisor(int b)
            {
                if (b == 0)
                    throw new DivideByZeroException("Cannot divide by zero");
            }
        }
    }
    
    // ===== EJEMPLO 2: VALIDADOR DE EMAIL =====
    namespace EmailValidatorExample
    {
        // PASO 1: RED - Pruebas para el validador de email
        public class EmailValidatorTests
        {
            [Test]
            public void IsValid_WithValidEmail_ReturnsTrue()
            {
                // Arrange
                var validator = new EmailValidator();
                var validEmails = new[]
                {
                    "test@example.com",
                    "user.name@domain.co.uk",
                    "user+tag@example.org"
                };
                
                // Act & Assert
                foreach (var email in validEmails)
                {
                    var result = validator.IsValid(email);
                    Assert.IsTrue(result, $"Email {email} should be valid");
                }
            }
            
            [Test]
            public void IsValid_WithInvalidEmail_ReturnsFalse()
            {
                // Arrange
                var validator = new EmailValidator();
                var invalidEmails = new[]
                {
                    "",
                    "invalid-email",
                    "@example.com",
                    "user@",
                    "user@.com",
                    "user name@example.com"
                };
                
                // Act & Assert
                foreach (var email in invalidEmails)
                {
                    var result = validator.IsValid(email);
                    Assert.IsFalse(result, $"Email {email} should be invalid");
                }
            }
            
            [Test]
            public void IsValid_WithNullEmail_ReturnsFalse()
            {
                // Arrange
                var validator = new EmailValidator();
                
                // Act
                var result = validator.IsValid(null);
                
                // Assert
                Assert.IsFalse(result);
            }
        }
        
        // PASO 2: GREEN - Implementación básica
        public class EmailValidator
        {
            public bool IsValid(string email)
            {
                if (string.IsNullOrEmpty(email))
                    return false;
                
                try
                {
                    var mailAddress = new System.Net.Mail.MailAddress(email);
                    return mailAddress.Address == email;
                }
                catch
                {
                    return false;
                }
            }
        }
        
        // PASO 3: REFACTOR - Implementación mejorada con regex
        public class EmailValidatorRefactored
        {
            private static readonly System.Text.RegularExpressions.Regex EmailRegex = 
                new System.Text.RegularExpressions.Regex(
                    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
                    System.Text.RegularExpressions.RegexOptions.Compiled);
            
            public bool IsValid(string email)
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;
                
                return EmailRegex.IsMatch(email);
            }
        }
    }
    
    // ===== EJEMPLO 3: GESTOR DE INVENTARIO =====
    namespace InventoryManagerExample
    {
        // PASO 1: RED - Pruebas para el gestor de inventario
        public class InventoryManagerTests
        {
            [Test]
            public void AddProduct_WithValidProduct_AddsToInventory()
            {
                // Arrange
                var manager = new InventoryManager();
                var product = new Product { Id = 1, Name = "Laptop", Stock = 10 };
                
                // Act
                manager.AddProduct(product);
                
                // Assert
                var retrievedProduct = manager.GetProduct(1);
                Assert.IsNotNull(retrievedProduct);
                Assert.AreEqual("Laptop", retrievedProduct.Name);
            }
            
            [Test]
            public void UpdateStock_WithValidQuantity_UpdatesStock()
            {
                // Arrange
                var manager = new InventoryManager();
                var product = new Product { Id = 1, Name = "Laptop", Stock = 10 };
                manager.AddProduct(product);
                
                // Act
                var success = manager.UpdateStock(1, 5);
                
                // Assert
                Assert.IsTrue(success);
                var updatedProduct = manager.GetProduct(1);
                Assert.AreEqual(5, updatedProduct.Stock);
            }
            
            [Test]
            public void UpdateStock_WithInsufficientStock_ReturnsFalse()
            {
                // Arrange
                var manager = new InventoryManager();
                var product = new Product { Id = 1, Name = "Laptop", Stock = 10 };
                manager.AddProduct(product);
                
                // Act
                var success = manager.UpdateStock(1, 15);
                
                // Assert
                Assert.IsFalse(success);
                var productAfterUpdate = manager.GetProduct(1);
                Assert.AreEqual(10, productAfterUpdate.Stock); // Stock no cambió
            }
            
            [Test]
            public void GetLowStockProducts_ReturnsProductsBelowThreshold()
            {
                // Arrange
                var manager = new InventoryManager();
                manager.AddProduct(new Product { Id = 1, Name = "Laptop", Stock = 5 });
                manager.AddProduct(new Product { Id = 2, Name = "Mouse", Stock = 15 });
                manager.AddProduct(new Product { Id = 3, Name = "Keyboard", Stock = 3 });
                
                // Act
                var lowStockProducts = manager.GetLowStockProducts(10);
                
                // Assert
                Assert.AreEqual(2, lowStockProducts.Count());
                Assert.IsTrue(lowStockProducts.Any(p => p.Name == "Laptop"));
                Assert.IsTrue(lowStockProducts.Any(p => p.Name == "Keyboard"));
            }
        }
        
        // PASO 2: GREEN - Implementación básica
        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Stock { get; set; }
        }
        
        public class InventoryManager
        {
            private readonly Dictionary<int, Product> _products = new();
            
            public void AddProduct(Product product)
            {
                if (product == null)
                    throw new ArgumentNullException(nameof(product));
                
                _products[product.Id] = product;
            }
            
            public Product GetProduct(int id)
            {
                return _products.TryGetValue(id, out var product) ? product : null;
            }
            
            public bool UpdateStock(int productId, int newStock)
            {
                if (newStock < 0)
                    return false;
                
                if (!_products.TryGetValue(productId, out var product))
                    return false;
                
                product.Stock = newStock;
                return true;
            }
            
            public IEnumerable<Product> GetLowStockProducts(int threshold)
            {
                return _products.Values.Where(p => p.Stock < threshold);
            }
        }
        
        // PASO 3: REFACTOR - Implementación mejorada
        public class InventoryManagerRefactored
        {
            private readonly Dictionary<int, Product> _products = new();
            private readonly ILogger _logger;
            
            public InventoryManagerRefactored(ILogger logger = null)
            {
                _logger = logger ?? new ConsoleLogger();
            }
            
            public void AddProduct(Product product)
            {
                ValidateProduct(product);
                _products[product.Id] = product;
                _logger.Log($"Product {product.Name} added to inventory");
            }
            
            public Product GetProduct(int id)
            {
                ValidateProductId(id);
                return _products.TryGetValue(id, out var product) ? product : null;
            }
            
            public bool UpdateStock(int productId, int newStock)
            {
                ValidateProductId(productId);
                ValidateStockQuantity(newStock);
                
                if (!_products.TryGetValue(productId, out var product))
                {
                    _logger.Log($"Product {productId} not found");
                    return false;
                }
                
                var oldStock = product.Stock;
                product.Stock = newStock;
                _logger.Log($"Stock updated for product {product.Name}: {oldStock} -> {newStock}");
                
                return true;
            }
            
            public IEnumerable<Product> GetLowStockProducts(int threshold)
            {
                ValidateThreshold(threshold);
                return _products.Values.Where(p => p.Stock < threshold).ToList();
            }
            
            private static void ValidateProduct(Product product)
            {
                if (product == null)
                    throw new ArgumentNullException(nameof(product));
                
                if (product.Id <= 0)
                    throw new ArgumentException("Product ID must be positive", nameof(product));
                
                if (string.IsNullOrWhiteSpace(product.Name))
                    throw new ArgumentException("Product name cannot be empty", nameof(product));
            }
            
            private static void ValidateProductId(int id)
            {
                if (id <= 0)
                    throw new ArgumentException("Product ID must be positive", nameof(id));
            }
            
            private static void ValidateStockQuantity(int quantity)
            {
                if (quantity < 0)
                    throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));
            }
            
            private static void ValidateThreshold(int threshold)
            {
                if (threshold < 0)
                    throw new ArgumentException("Threshold cannot be negative", nameof(threshold));
            }
        }
        
        public interface ILogger
        {
            void Log(string message);
        }
        
        public class ConsoleLogger : ILogger
        {
            public void Log(string message)
            {
                Console.WriteLine($"[LOG] {message}");
            }
        }
    }
    
    // ===== EJEMPLO 4: CALCULADORA DE IMPUESTOS =====
    namespace TaxCalculatorExample
    {
        // PASO 1: RED - Pruebas para la calculadora de impuestos
        public class TaxCalculatorTests
        {
            [Test]
            public void CalculateTax_WithBasicIncome_ReturnsCorrectTax()
            {
                // Arrange
                var calculator = new TaxCalculator();
                
                // Act & Assert
                Assert.AreEqual(0, calculator.CalculateTax(10000)); // Sin impuestos
                Assert.AreEqual(1000, calculator.CalculateTax(20000)); // 10% sobre 10000
                Assert.AreEqual(3000, calculator.CalculateTax(40000)); // 10% sobre 30000
            }
            
            [Test]
            public void CalculateTax_WithHighIncome_ReturnsCorrectTax()
            {
                // Arrange
                var calculator = new TaxCalculator();
                
                // Act & Assert
                Assert.AreEqual(5000, calculator.CalculateTax(50000)); // 10% sobre 50000
                Assert.AreEqual(12000, calculator.CalculateTax(80000)); // 10% sobre 50000 + 20% sobre 30000
            }
            
            [Test]
            public void CalculateTax_WithNegativeIncome_ThrowsException()
            {
                // Arrange
                var calculator = new TaxCalculator();
                
                // Act & Assert
                Assert.Throws<ArgumentException>(() => calculator.CalculateTax(-1000));
            }
            
            [Test]
            public void GetTaxBrackets_ReturnsCorrectBrackets()
            {
                // Arrange
                var calculator = new TaxCalculator();
                
                // Act
                var brackets = calculator.GetTaxBrackets();
                
                // Assert
                Assert.AreEqual(3, brackets.Count);
                Assert.AreEqual(0.10m, brackets[10000]);
                Assert.AreEqual(0.20m, brackets[50000]);
                Assert.AreEqual(0.30m, brackets[100000]);
            }
        }
        
        // PASO 2: GREEN - Implementación básica
        public class TaxCalculator
        {
            private readonly Dictionary<decimal, decimal> _taxBrackets = new()
            {
                { 10000, 0.10m },
                { 50000, 0.20m },
                { 100000, 0.30m }
            };
            
            public decimal CalculateTax(decimal income)
            {
                if (income < 0)
                    throw new ArgumentException("Income cannot be negative", nameof(income));
                
                if (income <= 10000)
                    return 0;
                
                decimal totalTax = 0;
                decimal remainingIncome = income;
                decimal previousBracket = 0;
                
                foreach (var bracket in _taxBrackets.OrderBy(b => b.Key))
                {
                    if (income > previousBracket)
                    {
                        var taxableAmount = Math.Min(income - previousBracket, bracket.Key - previousBracket);
                        totalTax += taxableAmount * bracket.Value;
                        previousBracket = bracket.Key;
                    }
                }
                
                return totalTax;
            }
            
            public Dictionary<decimal, decimal> GetTaxBrackets()
            {
                return new Dictionary<decimal, decimal>(_taxBrackets);
            }
        }
        
        // PASO 3: REFACTOR - Implementación mejorada
        public class TaxCalculatorRefactored
        {
            private readonly List<TaxBracket> _taxBrackets;
            
            public TaxCalculatorRefactored()
            {
                _taxBrackets = new List<TaxBracket>
                {
                    new TaxBracket(0, 10000, 0.0m),
                    new TaxBracket(10000, 50000, 0.10m),
                    new TaxBracket(50000, 100000, 0.20m),
                    new TaxBracket(100000, decimal.MaxValue, 0.30m)
                };
            }
            
            public decimal CalculateTax(decimal income)
            {
                ValidateIncome(income);
                
                if (income == 0)
                    return 0;
                
                var totalTax = 0.0m;
                var remainingIncome = income;
                
                foreach (var bracket in _taxBrackets)
                {
                    if (remainingIncome <= 0)
                        break;
                    
                    var taxableAmount = Math.Min(remainingIncome, bracket.MaxAmount - bracket.MinAmount);
                    totalTax += taxableAmount * bracket.Rate;
                    remainingIncome -= taxableAmount;
                }
                
                return Math.Round(totalTax, 2);
            }
            
            public List<TaxBracket> GetTaxBrackets()
            {
                return _taxBrackets.ToList();
            }
            
            private static void ValidateIncome(decimal income)
            {
                if (income < 0)
                    throw new ArgumentException("Income cannot be negative", nameof(income));
            }
        }
        
        public class TaxBracket
        {
            public decimal MinAmount { get; }
            public decimal MaxAmount { get; }
            public decimal Rate { get; }
            
            public TaxBracket(decimal minAmount, decimal maxAmount, decimal rate)
            {
                MinAmount = minAmount;
                MaxAmount = maxAmount;
                Rate = rate;
            }
        }
    }
    
    // ===== EJEMPLO 5: VALIDADOR DE CONTRASEÑAS =====
    namespace PasswordValidatorExample
    {
        // PASO 1: RED - Pruebas para el validador de contraseñas
        public class PasswordValidatorTests
        {
            [Test]
            public void IsValid_WithValidPassword_ReturnsTrue()
            {
                // Arrange
                var validator = new PasswordValidator();
                var validPasswords = new[]
                {
                    "StrongPass123!",
                    "MyP@ssw0rd",
                    "Secure123#"
                };
                
                // Act & Assert
                foreach (var password in validPasswords)
                {
                    var result = validator.IsValid(password);
                    Assert.IsTrue(result, $"Password {password} should be valid");
                }
            }
            
            [Test]
            public void IsValid_WithWeakPassword_ReturnsFalse()
            {
                // Arrange
                var validator = new PasswordValidator();
                var weakPasswords = new[]
                {
                    "weak",           // Muy corta
                    "password",        // Sin números ni caracteres especiales
                    "123456",          // Solo números
                    "abcdef",          // Solo letras minúsculas
                    "ABCDEF",          // Solo letras mayúsculas
                    "!@#$%^"          // Solo caracteres especiales
                };
                
                // Act & Assert
                foreach (var password in weakPasswords)
                {
                    var result = validator.IsValid(password);
                    Assert.IsFalse(result, $"Password {password} should be invalid");
                }
            }
            
            [Test]
            public void GetValidationErrors_WithWeakPassword_ReturnsErrors()
            {
                // Arrange
                var validator = new PasswordValidator();
                var weakPassword = "weak";
                
                // Act
                var errors = validator.GetValidationErrors(weakPassword);
                
                // Assert
                Assert.IsTrue(errors.Count > 0);
                Assert.IsTrue(errors.Any(e => e.Contains("length")));
                Assert.IsTrue(errors.Any(e => e.Contains("uppercase")));
                Assert.IsTrue(errors.Any(e => e.Contains("number")));
            }
        }
        
        // PASO 2: GREEN - Implementación básica
        public class PasswordValidator
        {
            private const int MinLength = 8;
            
            public bool IsValid(string password)
            {
                if (string.IsNullOrEmpty(password))
                    return false;
                
                return password.Length >= MinLength &&
                       password.Any(char.IsUpper) &&
                       password.Any(char.IsLower) &&
                       password.Any(char.IsDigit) &&
                       password.Any(IsSpecialCharacter);
            }
            
            public List<string> GetValidationErrors(string password)
            {
                var errors = new List<string>();
                
                if (string.IsNullOrEmpty(password))
                {
                    errors.Add("Password cannot be empty");
                    return errors;
                }
                
                if (password.Length < MinLength)
                    errors.Add($"Password must be at least {MinLength} characters long");
                
                if (!password.Any(char.IsUpper))
                    errors.Add("Password must contain at least one uppercase letter");
                
                if (!password.Any(char.IsLower))
                    errors.Add("Password must contain at least one lowercase letter");
                
                if (!password.Any(char.IsDigit))
                    errors.Add("Password must contain at least one number");
                
                if (!password.Any(IsSpecialCharacter))
                    errors.Add("Password must contain at least one special character");
                
                return errors;
            }
            
            private static bool IsSpecialCharacter(char c)
            {
                return "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c);
            }
        }
        
        // PASO 3: REFACTOR - Implementación mejorada
        public class PasswordValidatorRefactored
        {
            private readonly List<IPasswordRule> _rules;
            
            public PasswordValidatorRefactored()
            {
                _rules = new List<IPasswordRule>
                {
                    new MinimumLengthRule(8),
                    new UppercaseLetterRule(),
                    new LowercaseLetterRule(),
                    new NumberRule(),
                    new SpecialCharacterRule()
                };
            }
            
            public bool IsValid(string password)
            {
                return _rules.All(rule => rule.IsValid(password));
            }
            
            public List<string> GetValidationErrors(string password)
            {
                return _rules
                    .Where(rule => !rule.IsValid(password))
                    .Select(rule => rule.GetErrorMessage())
                    .ToList();
            }
        }
        
        public interface IPasswordRule
        {
            bool IsValid(string password);
            string GetErrorMessage();
        }
        
        public class MinimumLengthRule : IPasswordRule
        {
            private readonly int _minLength;
            
            public MinimumLengthRule(int minLength)
            {
                _minLength = minLength;
            }
            
            public bool IsValid(string password)
            {
                return password?.Length >= _minLength;
            }
            
            public string GetErrorMessage()
            {
                return $"Password must be at least {_minLength} characters long";
            }
        }
        
        public class UppercaseLetterRule : IPasswordRule
        {
            public bool IsValid(string password)
            {
                return password?.Any(char.IsUpper) ?? false;
            }
            
            public string GetErrorMessage()
            {
                return "Password must contain at least one uppercase letter";
            }
        }
        
        public class LowercaseLetterRule : IPasswordRule
        {
            public bool IsValid(string password)
            {
                return password?.Any(char.IsLower) ?? false;
            }
            
            public string GetErrorMessage()
            {
                return "Password must contain at least one lowercase letter";
            }
        }
        
        public class NumberRule : IPasswordRule
        {
            public bool IsValid(string password)
            {
                return password?.Any(char.IsDigit) ?? false;
            }
            
            public string GetErrorMessage()
            {
                return "Password must contain at least one number";
            }
        }
        
        public class SpecialCharacterRule : IPasswordRule
        {
            private static readonly char[] SpecialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?".ToCharArray();
            
            public bool IsValid(string password)
            {
                return password?.Any(c => SpecialCharacters.Contains(c)) ?? false;
            }
            
            public string GetErrorMessage()
            {
                return "Password must contain at least one special character";
            }
        }
    }
}

// ===== DEMOSTRACIÓN DE TDD =====
public class TDDDemonstration
{
    public static void DemonstrateTDD()
    {
        Console.WriteLine("=== Desarrollo Dirigido por Pruebas (TDD) - Clase 2 ===\n");
        
        // Ejemplo 1: Calculadora
        Console.WriteLine("1. CALCULADORA:");
        var calculator = new TDDDevelopment.CalculatorExample.Calculator();
        Console.WriteLine($"2 + 3 = {calculator.Add(2, 3)}");
        Console.WriteLine($"5 - 3 = {calculator.Subtract(5, 3)}");
        Console.WriteLine($"4 * 3 = {calculator.Multiply(4, 3)}");
        Console.WriteLine($"10 / 2 = {calculator.Divide(10, 2)}");
        
        // Ejemplo 2: Validador de Email
        Console.WriteLine("\n2. VALIDADOR DE EMAIL:");
        var emailValidator = new TDDDevelopment.EmailValidatorExample.EmailValidator();
        var testEmails = new[] { "test@example.com", "invalid-email", "" };
        foreach (var email in testEmails)
        {
            Console.WriteLine($"'{email}' es válido: {emailValidator.IsValid(email)}");
        }
        
        // Ejemplo 3: Gestor de Inventario
        Console.WriteLine("\n3. GESTOR DE INVENTARIO:");
        var inventoryManager = new TDDDevelopment.InventoryManagerExample.InventoryManager();
        inventoryManager.AddProduct(new TDDDevelopment.InventoryManagerExample.Product { Id = 1, Name = "Laptop", Stock = 10 });
        inventoryManager.AddProduct(new TDDDevelopment.InventoryManagerExample.Product { Id = 2, Name = "Mouse", Stock = 5 });
        
        var lowStockProducts = inventoryManager.GetLowStockProducts(8);
        Console.WriteLine($"Productos con stock bajo: {string.Join(", ", lowStockProducts.Select(p => p.Name))}");
        
        // Ejemplo 4: Calculadora de Impuestos
        Console.WriteLine("\n4. CALCULADORA DE IMPUESTOS:");
        var taxCalculator = new TDDDevelopment.TaxCalculatorExample.TaxCalculator();
        var incomes = new[] { 10000, 20000, 40000, 80000 };
        foreach (var income in incomes)
        {
            var tax = taxCalculator.CalculateTax(income);
            Console.WriteLine($"Ingreso: ${income}, Impuesto: ${tax}");
        }
        
        // Ejemplo 5: Validador de Contraseñas
        Console.WriteLine("\n5. VALIDADOR DE CONTRASEÑAS:");
        var passwordValidator = new TDDDevelopment.PasswordValidatorExample.PasswordValidator();
        var testPasswords = new[] { "StrongPass123!", "weak", "password" };
        foreach (var password in testPasswords)
        {
            var isValid = passwordValidator.IsValid(password);
            Console.WriteLine($"'{password}' es válida: {isValid}");
            
            if (!isValid)
            {
                var errors = passwordValidator.GetValidationErrors(password);
                Console.WriteLine($"  Errores: {string.Join(", ", errors)}");
            }
        }
        
        Console.WriteLine("\n✅ TDD comprendido!");
        Console.WriteLine("Recuerda: Red -> Green -> Refactor");
    }
}

// Programa principal
public class Program
{
    public static async Task Main()
    {
        TDDDemonstration.DemonstrateTDD();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Calculadora de Descuentos
Implementa usando TDD una calculadora que:
- Calcule descuentos por cantidad
- Aplique descuentos por cliente VIP
- Valide rangos de descuento

### Ejercicio 2: Validador de Formularios
Crea usando TDD un validador que verifique:
- Nombres (solo letras y espacios)
- Edades (18-120 años)
- Números de teléfono

### Ejercicio 3: Gestor de Tareas
Implementa usando TDD un sistema que:
- Agregue tareas con prioridad
- Marque tareas como completadas
- Filtre tareas por estado

## 🔍 Puntos Clave

1. **Ciclo Red-Green-Refactor** es el corazón del TDD
2. **Escribir pruebas primero** ayuda a clarificar requisitos
3. **Implementación mínima** para que las pruebas pasen
4. **Refactoring continuo** mejora la calidad del código
5. **Pruebas como documentación** del comportamiento esperado
6. **Diseño emergente** a través del proceso TDD
7. **Cobertura de código** garantizada por las pruebas
8. **Mantenibilidad** mejorada con TDD

## 📚 Recursos Adicionales

- [TDD by Example - Kent Beck](https://www.amazon.com/Test-Driven-Development-Kent-Beck/dp/0321146530)
- [Microsoft Docs - TDD](https://docs.microsoft.com/en-us/visualstudio/test/quick-start-test-driven-development-with-test-explorer)
- [TDD Best Practices](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/test-aspnet-core-mvc)

---

**🎯 ¡Has completado la Clase 2! Ahora comprendes el Desarrollo Dirigido por Pruebas (TDD)**

**📚 [Siguiente: Clase 3 - Testing Unitario](clase_3_testing_unitario.md)**
