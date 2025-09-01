# 🚀 Clase 3: Testing Unitario

## 📋 Información de la Clase

- **Módulo**: Senior Level 2 - Testing y TDD
- **Duración**: 2 horas
- **Nivel**: Avanzado
- **Prerrequisitos**: Desarrollo Dirigido por Pruebas (Clase 2)

## 🎯 Objetivos de Aprendizaje

- Implementar testing unitario efectivo
- Usar frameworks de testing (xUnit, NUnit, MSTest)
- Crear pruebas aisladas y mantenibles
- Aplicar patrones de testing unitario

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_fundamentos_testing.md) | Fundamentos de Testing | |
| [Clase 2](clase_2_desarrollo_dirigido_pruebas.md) | Desarrollo Dirigido por Pruebas (TDD) | ← Anterior |
| **Clase 3** | **Testing Unitario** | ← Estás aquí |
| [Clase 4](clase_4_testing_integracion.md) | Testing de Integración | Siguiente → |
| [Clase 5](clase_5_testing_comportamiento.md) | Testing de Comportamiento | |
| [Clase 6](clase_6_mocking_frameworks.md) | Frameworks de Mocking | |
| [Clase 7](clase_7_testing_asincrono.md) | Testing de Código Asíncrono | |
| [Clase 8](clase_8_testing_apis.md) | Testing de APIs | |
| [Clase 9](clase_9_testing_database.md) | Testing de Base de Datos | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Testing | |

**← [Volver al README del Módulo 2](../senior_2/README.md)**

---

## 📚 Contenido Teórico

### 1. ¿Qué es el Testing Unitario?

El testing unitario verifica que una unidad individual de código (método, clase, función) funcione correctamente de forma aislada.

### 2. Características del Testing Unitario

- **Rápido**: Ejecución en milisegundos
- **Aislado**: No depende de sistemas externos
- **Determinístico**: Mismo resultado siempre
- **Mantenible**: Fácil de entender y modificar

```csharp
// ===== TESTING UNITARIO - IMPLEMENTACIÓN COMPLETA =====
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTesting
{
    // ===== EJEMPLO 1: CALCULADORA CON TESTING UNITARIO =====
    namespace CalculatorUnitTests
    {
        // Clase principal a probar
        public class Calculator
        {
            public int Add(int a, int b) => a + b;
            public int Subtract(int a, int b) => a - b;
            public int Multiply(int a, int b) => a * b;
            public int Divide(int a, int b)
            {
                if (b == 0)
                    throw new DivideByZeroException("Cannot divide by zero");
                return a / b;
            }
            
            public double Power(double baseNumber, double exponent) => Math.Pow(baseNumber, exponent);
            public int Modulo(int a, int b)
            {
                if (b == 0)
                    throw new DivideByZeroException("Cannot calculate modulo with zero");
                return a % b;
            }
            
            public bool IsEven(int number) => number % 2 == 0;
            public bool IsPrime(int number)
            {
                if (number < 2) return false;
                if (number == 2) return true;
                if (number % 2 == 0) return false;
                
                for (int i = 3; i <= Math.Sqrt(number); i += 2)
                {
                    if (number % i == 0) return false;
                }
                return true;
            }
        }
        
        // Pruebas unitarias usando xUnit
        public class CalculatorTests
        {
            private readonly Calculator _calculator;
            
            public CalculatorTests()
            {
                _calculator = new Calculator();
            }
            
            [Fact]
            public void Add_WithTwoPositiveNumbers_ReturnsSum()
            {
                // Arrange
                var a = 5;
                var b = 3;
                var expected = 8;
                
                // Act
                var result = _calculator.Add(a, b);
                
                // Assert
                Assert.Equal(expected, result);
            }
            
            [Fact]
            public void Add_WithNegativeNumbers_ReturnsCorrectSum()
            {
                // Arrange
                var a = -5;
                var b = -3;
                var expected = -8;
                
                // Act
                var result = _calculator.Add(a, b);
                
                // Assert
                Assert.Equal(expected, result);
            }
            
            [Fact]
            public void Add_WithZero_ReturnsOtherNumber()
            {
                // Arrange
                var a = 5;
                var b = 0;
                var expected = 5;
                
                // Act
                var result = _calculator.Add(a, b);
                
                // Assert
                Assert.Equal(expected, result);
            }
            
            [Theory]
            [InlineData(5, 3, 2)]
            [InlineData(10, 7, 3)]
            [InlineData(0, 5, -5)]
            [InlineData(-5, -3, -2)]
            public void Subtract_WithVariousNumbers_ReturnsCorrectDifference(int a, int b, int expected)
            {
                // Act
                var result = _calculator.Subtract(a, b);
                
                // Assert
                Assert.Equal(expected, result);
            }
            
            [Theory]
            [InlineData(4, 3, 12)]
            [InlineData(0, 5, 0)]
            [InlineData(-2, 3, -6)]
            [InlineData(-2, -3, 6)]
            public void Multiply_WithVariousNumbers_ReturnsCorrectProduct(int a, int b, int expected)
            {
                // Act
                var result = _calculator.Multiply(a, b);
                
                // Assert
                Assert.Equal(expected, result);
            }
            
            [Theory]
            [InlineData(10, 2, 5)]
            [InlineData(15, 3, 5)]
            [InlineData(0, 5, 0)]
            [InlineData(-10, 2, -5)]
            public void Divide_WithValidDivisors_ReturnsCorrectQuotient(int a, int b, int expected)
            {
                // Act
                var result = _calculator.Divide(a, b);
                
                // Assert
                Assert.Equal(expected, result);
            }
            
            [Fact]
            public void Divide_ByZero_ThrowsDivideByZeroException()
            {
                // Arrange
                var a = 10;
                var b = 0;
                
                // Act & Assert
                var exception = Assert.Throws<DivideByZeroException>(() => _calculator.Divide(a, b));
                Assert.Equal("Cannot divide by zero", exception.Message);
            }
            
            [Theory]
            [InlineData(2, 3, 8)]
            [InlineData(5, 0, 1)]
            [InlineData(2, -1, 0.5)]
            [InlineData(0, 5, 0)]
            public void Power_WithVariousExponents_ReturnsCorrectResult(double baseNumber, double exponent, double expected)
            {
                // Act
                var result = _calculator.Power(baseNumber, exponent);
                
                // Assert
                Assert.Equal(expected, result, 5);
            }
            
            [Theory]
            [InlineData(10, 3, 1)]
            [InlineData(15, 4, 3)]
            [InlineData(20, 5, 0)]
            public void Modulo_WithValidDivisors_ReturnsCorrectRemainder(int a, int b, int expected)
            {
                // Act
                var result = _calculator.Modulo(a, b);
                
                // Assert
                Assert.Equal(expected, result);
            }
            
            [Fact]
            public void Modulo_ByZero_ThrowsDivideByZeroException()
            {
                // Arrange
                var a = 10;
                var b = 0;
                
                // Act & Assert
                var exception = Assert.Throws<DivideByZeroException>(() => _calculator.Modulo(a, b));
                Assert.Equal("Cannot calculate modulo with zero", exception.Message);
            }
            
            [Theory]
            [InlineData(2, true)]
            [InlineData(4, true)]
            [InlineData(6, true)]
            [InlineData(1, false)]
            [InlineData(3, false)]
            [InlineData(5, false)]
            public void IsEven_WithVariousNumbers_ReturnsCorrectResult(int number, bool expected)
            {
                // Act
                var result = _calculator.IsEven(number);
                
                // Assert
                Assert.Equal(expected, result);
            }
            
            [Theory]
            [InlineData(2, true)]
            [InlineData(3, true)]
            [InlineData(5, true)]
            [InlineData(7, true)]
            [InlineData(11, true)]
            [InlineData(4, false)]
            [InlineData(6, false)]
            [InlineData(8, false)]
            [InlineData(9, false)]
            [InlineData(1, false)]
            [InlineData(0, false)]
            [InlineData(-1, false)]
            public void IsPrime_WithVariousNumbers_ReturnsCorrectResult(int number, bool expected)
            {
                // Act
                var result = _calculator.IsPrime(number);
                
                // Assert
                Assert.Equal(expected, result);
            }
        }
    }
    
    // ===== EJEMPLO 2: VALIDADOR DE EMAIL CON TESTING UNITARIO =====
    namespace EmailValidatorUnitTests
    {
        public class EmailValidator
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
            
            public List<string> GetValidationErrors(string email)
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(email))
                {
                    errors.Add("Email cannot be empty");
                    return errors;
                }
                
                if (!email.Contains("@"))
                    errors.Add("Email must contain @ symbol");
                
                if (!email.Contains("."))
                    errors.Add("Email must contain domain extension");
                
                if (email.StartsWith("@") || email.EndsWith("@"))
                    errors.Add("Email cannot start or end with @ symbol");
                
                if (email.StartsWith(".") || email.EndsWith("."))
                    errors.Add("Email cannot start or end with dot");
                
                if (!EmailRegex.IsMatch(email))
                    errors.Add("Email format is invalid");
                
                return errors;
            }
        }
        
        public class EmailValidatorTests
        {
            private readonly EmailValidator _validator;
            
            public EmailValidatorTests()
            {
                _validator = new EmailValidator();
            }
            
            [Theory]
            [InlineData("test@example.com")]
            [InlineData("user.name@domain.co.uk")]
            [InlineData("user+tag@example.org")]
            [InlineData("user123@test-domain.com")]
            [InlineData("a@b.c")]
            public void IsValid_WithValidEmails_ReturnsTrue(string email)
            {
                // Act
                var result = _validator.IsValid(email);
                
                // Assert
                Assert.True(result, $"Email '{email}' should be valid");
            }
            
            [Theory]
            [InlineData("")]
            [InlineData("   ")]
            [InlineData(null)]
            [InlineData("invalid-email")]
            [InlineData("@example.com")]
            [InlineData("user@")]
            [InlineData("user@.com")]
            [InlineData("user name@example.com")]
            [InlineData("user@example")]
            [InlineData("user..name@example.com")]
            [InlineData("user@example..com")]
            public void IsValid_WithInvalidEmails_ReturnsFalse(string email)
            {
                // Act
                var result = _validator.IsValid(email);
                
                // Assert
                Assert.False(result, $"Email '{email}' should be invalid");
            }
            
            [Fact]
            public void GetValidationErrors_WithEmptyEmail_ReturnsEmptyError()
            {
                // Arrange
                var email = "";
                
                // Act
                var errors = _validator.GetValidationErrors(email);
                
                // Assert
                Assert.Single(errors);
                Assert.Contains("Email cannot be empty", errors);
            }
            
            [Fact]
            public void GetValidationErrors_WithNullEmail_ReturnsEmptyError()
            {
                // Arrange
                string email = null;
                
                // Act
                var errors = _validator.GetValidationErrors(email);
                
                // Assert
                Assert.Single(errors);
                Assert.Contains("Email cannot be empty", errors);
            }
            
            [Fact]
            public void GetValidationErrors_WithEmailWithoutAtSymbol_ReturnsAtSymbolError()
            {
                // Arrange
                var email = "invalid-email";
                
                // Act
                var errors = _validator.GetValidationErrors(email);
                
                // Assert
                Assert.Contains("Email must contain @ symbol", errors);
            }
            
            [Fact]
            public void GetValidationErrors_WithEmailWithoutDomain_ReturnsDomainError()
            {
                // Arrange
                var email = "user@";
                
                // Act
                var errors = _validator.GetValidationErrors(email);
                
                // Assert
                Assert.Contains("Email must contain domain extension", errors);
            }
            
            [Fact]
            public void GetValidationErrors_WithEmailStartingWithAt_ReturnsAtSymbolError()
            {
                // Arrange
                var email = "@example.com";
                
                // Act
                var errors = _validator.GetValidationErrors(email);
                
                // Assert
                Assert.Contains("Email cannot start or end with @ symbol", errors);
            }
            
            [Fact]
            public void GetValidationErrors_WithEmailEndingWithAt_ReturnsAtSymbolError()
            {
                // Arrange
                var email = "user@";
                
                // Act
                var errors = _validator.GetValidationErrors(email);
                
                // Assert
                Assert.Contains("Email cannot start or end with @ symbol", errors);
            }
            
            [Fact]
            public void GetValidationErrors_WithEmailStartingWithDot_ReturnsDotError()
            {
                // Arrange
                var email = ".user@example.com";
                
                // Act
                var errors = _validator.GetValidationErrors(email);
                
                // Assert
                Assert.Contains("Email cannot start or end with dot", errors);
            }
            
            [Fact]
            public void GetValidationErrors_WithEmailEndingWithDot_ReturnsDotError()
            {
                // Arrange
                var email = "user@example.com.";
                
                // Act
                var errors = _validator.GetValidationErrors(email);
                
                // Assert
                Assert.Contains("Email cannot start or end with dot", errors);
            }
            
            [Fact]
            public void GetValidationErrors_WithValidEmail_ReturnsNoErrors()
            {
                // Arrange
                var email = "test@example.com";
                
                // Act
                var errors = _validator.GetValidationErrors(email);
                
                // Assert
                Assert.Empty(errors);
            }
        }
    }
    
    // ===== EJEMPLO 3: GESTOR DE INVENTARIO CON TESTING UNITARIO =====
    namespace InventoryManagerUnitTests
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
        
        public interface IProductRepository
        {
            Task<Product> GetByIdAsync(int id);
            Task<IEnumerable<Product>> GetAllAsync();
            Task<Product> CreateAsync(Product product);
            Task<bool> UpdateAsync(Product product);
            Task<bool> DeleteAsync(int id);
        }
        
        public class InventoryManager
        {
            private readonly IProductRepository _repository;
            
            public InventoryManager(IProductRepository repository)
            {
                _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            }
            
            public async Task<Product> GetProductAsync(int id)
            {
                if (id <= 0)
                    throw new ArgumentException("Product ID must be positive", nameof(id));
                
                return await _repository.GetByIdAsync(id);
            }
            
            public async Task<IEnumerable<Product>> GetAllProductsAsync()
            {
                return await _repository.GetAllAsync();
            }
            
            public async Task<Product> AddProductAsync(Product product)
            {
                ValidateProduct(product);
                return await _repository.CreateAsync(product);
            }
            
            public async Task<bool> UpdateProductAsync(Product product)
            {
                ValidateProduct(product);
                return await _repository.UpdateAsync(product);
            }
            
            public async Task<bool> DeleteProductAsync(int id)
            {
                if (id <= 0)
                    throw new ArgumentException("Product ID must be positive", nameof(id));
                
                return await _repository.DeleteAsync(id);
            }
            
            public async Task<bool> UpdateStockAsync(int productId, int newStock)
            {
                if (productId <= 0)
                    throw new ArgumentException("Product ID must be positive", nameof(productId));
                
                if (newStock < 0)
                    throw new ArgumentException("Stock cannot be negative", nameof(newStock));
                
                var product = await _repository.GetByIdAsync(productId);
                if (product == null)
                    return false;
                
                product.Stock = newStock;
                return await _repository.UpdateAsync(product);
            }
            
            public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
            {
                if (threshold < 0)
                    throw new ArgumentException("Threshold cannot be negative", nameof(threshold));
                
                var products = await _repository.GetAllAsync();
                return products.Where(p => p.Stock < threshold && p.IsActive);
            }
            
            public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
            {
                if (string.IsNullOrWhiteSpace(category))
                    throw new ArgumentException("Category cannot be null or empty", nameof(category));
                
                var products = await _repository.GetAllAsync();
                return products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && p.IsActive);
            }
            
            public async Task<decimal> CalculateTotalInventoryValueAsync()
            {
                var products = await _repository.GetAllAsync();
                return products.Where(p => p.IsActive).Sum(p => p.Price * p.Stock);
            }
            
            private static void ValidateProduct(Product product)
            {
                if (product == null)
                    throw new ArgumentNullException(nameof(product));
                
                if (product.Id <= 0)
                    throw new ArgumentException("Product ID must be positive", nameof(product));
                
                if (string.IsNullOrWhiteSpace(product.Name))
                    throw new ArgumentException("Product name cannot be null or empty", nameof(product));
                
                if (product.Price < 0)
                    throw new ArgumentException("Product price cannot be negative", nameof(product));
                
                if (product.Stock < 0)
                    throw new ArgumentException("Product stock cannot be negative", nameof(product));
            }
        }
        
        // Mock del repositorio para testing
        public class MockProductRepository : IProductRepository
        {
            private readonly Dictionary<int, Product> _products = new();
            private int _nextId = 1;
            
            public Task<Product> GetByIdAsync(int id)
            {
                return Task.FromResult(_products.TryGetValue(id, out var product) ? product : null);
            }
            
            public Task<IEnumerable<Product>> GetAllAsync()
            {
                return Task.FromResult(_products.Values.AsEnumerable());
            }
            
            public Task<Product> CreateAsync(Product product)
            {
                var newProduct = new Product(_nextId, product.Name, product.Price, product.Stock, product.Category)
                {
                    IsActive = product.IsActive
                };
                
                _products[_nextId] = newProduct;
                _nextId++;
                
                return Task.FromResult(newProduct);
            }
            
            public Task<bool> UpdateAsync(Product product)
            {
                if (_products.ContainsKey(product.Id))
                {
                    _products[product.Id] = product;
                    return Task.FromResult(true);
                }
                
                return Task.FromResult(false);
            }
            
            public Task<bool> DeleteAsync(int id)
            {
                return Task.FromResult(_products.Remove(id));
            }
        }
        
        public class InventoryManagerTests
        {
            private readonly InventoryManager _manager;
            private readonly MockProductRepository _mockRepository;
            
            public InventoryManagerTests()
            {
                _mockRepository = new MockProductRepository();
                _manager = new InventoryManager(_mockRepository);
            }
            
            [Fact]
            public async Task GetProductAsync_WithValidId_ReturnsProduct()
            {
                // Arrange
                var product = new Product(1, "Test Product", 10.99m, 5, "Test Category");
                await _mockRepository.CreateAsync(product);
                
                // Act
                var result = await _manager.GetProductAsync(1);
                
                // Assert
                Assert.NotNull(result);
                Assert.Equal("Test Product", result.Name);
                Assert.Equal(10.99m, result.Price);
            }
            
            [Fact]
            public async Task GetProductAsync_WithInvalidId_ThrowsArgumentException()
            {
                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => _manager.GetProductAsync(0));
                await Assert.ThrowsAsync<ArgumentException>(() => _manager.GetProductAsync(-1));
            }
            
            [Fact]
            public async Task GetProductAsync_WithNonExistentId_ReturnsNull()
            {
                // Act
                var result = await _manager.GetProductAsync(999);
                
                // Assert
                Assert.Null(result);
            }
            
            [Fact]
            public async Task AddProductAsync_WithValidProduct_ReturnsCreatedProduct()
            {
                // Arrange
                var product = new Product(0, "New Product", 15.99m, 10, "New Category");
                
                // Act
                var result = await _manager.AddProductAsync(product);
                
                // Assert
                Assert.NotNull(result);
                Assert.NotEqual(0, result.Id);
                Assert.Equal("New Product", result.Name);
                Assert.Equal(15.99m, result.Price);
            }
            
            [Fact]
            public async Task AddProductAsync_WithNullProduct_ThrowsArgumentNullException()
            {
                // Arrange
                Product product = null;
                
                // Act & Assert
                await Assert.ThrowsAsync<ArgumentNullException>(() => _manager.AddProductAsync(product));
            }
            
            [Fact]
            public async Task AddProductAsync_WithInvalidProduct_ThrowsArgumentException()
            {
                // Arrange
                var product = new Product(0, "", -10.99m, -5, "Test Category");
                
                // Act & Assert
                var exception = await Assert.ThrowsAsync<ArgumentException>(() => _manager.AddProductAsync(product));
                Assert.Contains("Product name cannot be null or empty", exception.Message);
            }
            
            [Fact]
            public async Task UpdateStockAsync_WithValidParameters_ReturnsTrue()
            {
                // Arrange
                var product = new Product(1, "Test Product", 10.99m, 5, "Test Category");
                await _mockRepository.CreateAsync(product);
                
                // Act
                var result = await _manager.UpdateStockAsync(1, 10);
                
                // Assert
                Assert.True(result);
                
                var updatedProduct = await _manager.GetProductAsync(1);
                Assert.Equal(10, updatedProduct.Stock);
            }
            
            [Fact]
            public async Task UpdateStockAsync_WithInvalidProductId_ThrowsArgumentException()
            {
                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => _manager.UpdateStockAsync(0, 10));
                await Assert.ThrowsAsync<ArgumentException>(() => _manager.UpdateStockAsync(-1, 10));
            }
            
            [Fact]
            public async Task UpdateStockAsync_WithNegativeStock_ThrowsArgumentException()
            {
                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => _manager.UpdateStockAsync(1, -5));
            }
            
            [Fact]
            public async Task UpdateStockAsync_WithNonExistentProduct_ReturnsFalse()
            {
                // Act
                var result = await _manager.UpdateStockAsync(999, 10);
                
                // Assert
                Assert.False(result);
            }
            
            [Fact]
            public async Task GetLowStockProductsAsync_WithValidThreshold_ReturnsLowStockProducts()
            {
                // Arrange
                await _mockRepository.CreateAsync(new Product(0, "Product 1", 10.99m, 3, "Category 1"));
                await _mockRepository.CreateAsync(new Product(0, "Product 2", 15.99m, 15, "Category 1"));
                await _mockRepository.CreateAsync(new Product(0, "Product 3", 20.99m, 7, "Category 2"));
                
                // Act
                var lowStockProducts = await _manager.GetLowStockProductsAsync(10);
                
                // Assert
                Assert.Equal(2, lowStockProducts.Count());
                Assert.Contains(lowStockProducts, p => p.Name == "Product 1");
                Assert.Contains(lowStockProducts, p => p.Name == "Product 3");
            }
            
            [Fact]
            public async Task GetLowStockProductsAsync_WithNegativeThreshold_ThrowsArgumentException()
            {
                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => _manager.GetLowStockProductsAsync(-5));
            }
            
            [Fact]
            public async Task GetProductsByCategoryAsync_WithValidCategory_ReturnsProductsInCategory()
            {
                // Arrange
                await _mockRepository.CreateAsync(new Product(0, "Product 1", 10.99m, 5, "Electronics"));
                await _mockRepository.CreateAsync(new Product(0, "Product 2", 15.99m, 10, "Books"));
                await _mockRepository.CreateAsync(new Product(0, "Product 3", 20.99m, 8, "Electronics"));
                
                // Act
                var electronicsProducts = await _manager.GetProductsByCategoryAsync("Electronics");
                
                // Assert
                Assert.Equal(2, electronicsProducts.Count());
                Assert.All(electronicsProducts, p => Assert.Equal("Electronics", p.Category));
            }
            
            [Fact]
            public async Task GetProductsByCategoryAsync_WithNullCategory_ThrowsArgumentException()
            {
                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => _manager.GetProductsByCategoryAsync(null));
            }
            
            [Fact]
            public async Task GetProductsByCategoryAsync_WithEmptyCategory_ThrowsArgumentException()
            {
                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => _manager.GetProductsByCategoryAsync(""));
                await Assert.ThrowsAsync<ArgumentException>(() => _manager.GetProductsByCategoryAsync("   "));
            }
            
            [Fact]
            public async Task CalculateTotalInventoryValueAsync_WithProducts_ReturnsCorrectValue()
            {
                // Arrange
                await _mockRepository.CreateAsync(new Product(0, "Product 1", 10.00m, 5, "Category 1"));
                await _mockRepository.CreateAsync(new Product(0, "Product 2", 20.00m, 3, "Category 1"));
                await _mockRepository.CreateAsync(new Product(0, "Product 3", 15.00m, 2, "Category 2"));
                
                // Expected: (10 * 5) + (20 * 3) + (15 * 2) = 50 + 60 + 30 = 140
                var expectedValue = 140.00m;
                
                // Act
                var result = await _manager.CalculateTotalInventoryValueAsync();
                
                // Assert
                Assert.Equal(expectedValue, result);
            }
            
            [Fact]
            public async Task CalculateTotalInventoryValueAsync_WithNoProducts_ReturnsZero()
            {
                // Act
                var result = await _manager.CalculateTotalInventoryValueAsync();
                
                // Assert
                Assert.Equal(0, result);
            }
        }
    }
    
    // ===== EJEMPLO 4: VALIDADOR DE CONTRASEÑAS CON TESTING UNITARIO =====
    namespace PasswordValidatorUnitTests
    {
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
        
        public class PasswordValidator
        {
            private readonly List<IPasswordRule> _rules;
            
            public PasswordValidator()
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
            
            public PasswordValidator(IEnumerable<IPasswordRule> customRules)
            {
                _rules = customRules?.ToList() ?? new List<IPasswordRule>();
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
            
            public int GetStrengthScore(string password)
            {
                if (string.IsNullOrEmpty(password))
                    return 0;
                
                var score = 0;
                
                if (password.Length >= 8) score += 1;
                if (password.Length >= 12) score += 1;
                if (password.Any(char.IsUpper)) score += 1;
                if (password.Any(char.IsLower)) score += 1;
                if (password.Any(char.IsDigit)) score += 1;
                if (password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c))) score += 1;
                
                return score;
            }
        }
        
        public class PasswordValidatorTests
        {
            private readonly PasswordValidator _validator;
            
            public PasswordValidatorTests()
            {
                _validator = new PasswordValidator();
            }
            
            [Theory]
            [InlineData("StrongPass123!")]
            [InlineData("MyP@ssw0rd")]
            [InlineData("Secure123#")]
            [InlineData("ComplexP@ss1")]
            public void IsValid_WithValidPasswords_ReturnsTrue(string password)
            {
                // Act
                var result = _validator.IsValid(password);
                
                // Assert
                Assert.True(result, $"Password '{password}' should be valid");
            }
            
            [Theory]
            [InlineData("")]
            [InlineData(null)]
            [InlineData("weak")]
            [InlineData("password")]
            [InlineData("123456")]
            [InlineData("abcdef")]
            [InlineData("ABCDEF")]
            [InlineData("!@#$%^")]
            [InlineData("Weak1")]
            [InlineData("weakpass")]
            [InlineData("WEAKPASS")]
            [InlineData("weakpass1")]
            public void IsValid_WithInvalidPasswords_ReturnsFalse(string password)
            {
                // Act
                var result = _validator.IsValid(password);
                
                // Assert
                Assert.False(result, $"Password '{password}' should be invalid");
            }
            
            [Fact]
            public void GetValidationErrors_WithEmptyPassword_ReturnsAllErrors()
            {
                // Arrange
                var password = "";
                
                // Act
                var errors = _validator.GetValidationErrors(password);
                
                // Assert
                Assert.Equal(5, errors.Count);
                Assert.Contains("Password must be at least 8 characters long", errors);
                Assert.Contains("Password must contain at least one uppercase letter", errors);
                Assert.Contains("Password must contain at least one lowercase letter", errors);
                Assert.Contains("Password must contain at least one number", errors);
                Assert.Contains("Password must contain at least one special character", errors);
            }
            
            [Fact]
            public void GetValidationErrors_WithWeakPassword_ReturnsSpecificErrors()
            {
                // Arrange
                var password = "weak";
                
                // Act
                var errors = _validator.GetValidationErrors(password);
                
                // Assert
                Assert.Equal(4, errors.Count);
                Assert.Contains("Password must be at least 8 characters long", errors);
                Assert.Contains("Password must contain at least one uppercase letter", errors);
                Assert.Contains("Password must contain at least one number", errors);
                Assert.Contains("Password must contain at least one special character", errors);
            }
            
            [Fact]
            public void GetValidationErrors_WithValidPassword_ReturnsNoErrors()
            {
                // Arrange
                var password = "StrongPass123!";
                
                // Act
                var errors = _validator.GetValidationErrors(password);
                
                // Assert
                Assert.Empty(errors);
            }
            
            [Theory]
            [InlineData("", 0)]
            [InlineData("weak", 0)]
            [InlineData("Weak1", 1)]
            [InlineData("weakpass", 1)]
            [InlineData("WeakPass", 2)]
            [InlineData("WeakPass1", 3)]
            [InlineData("WeakPass1!", 4)]
            [InlineData("StrongPass123!", 6)]
            public void GetStrengthScore_WithVariousPasswords_ReturnsCorrectScore(string password, int expectedScore)
            {
                // Act
                var score = _validator.GetStrengthScore(password);
                
                // Assert
                Assert.Equal(expectedScore, score);
            }
            
            [Fact]
            public void Constructor_WithCustomRules_UsesCustomRules()
            {
                // Arrange
                var customRules = new List<IPasswordRule>
                {
                    new MinimumLengthRule(6),
                    new UppercaseLetterRule()
                };
                
                var customValidator = new PasswordValidator(customRules);
                
                // Act
                var isValid = customValidator.IsValid("Test12");
                var errors = customValidator.GetValidationErrors("test");
                
                // Assert
                Assert.True(isValid);
                Assert.Equal(2, errors.Count);
                Assert.Contains("Password must be at least 6 characters long", errors);
                Assert.Contains("Password must contain at least one uppercase letter", errors);
            }
        }
    }
}

// ===== DEMOSTRACIÓN DE TESTING UNITARIO =====
public class UnitTestingDemonstration
{
    public static void DemonstrateUnitTesting()
    {
        Console.WriteLine("=== Testing Unitario - Clase 3 ===\n");
        
        // Ejemplo 1: Calculadora
        Console.WriteLine("1. CALCULADORA:");
        var calculator = new UnitTesting.CalculatorUnitTests.Calculator();
        Console.WriteLine($"2 + 3 = {calculator.Add(2, 3)}");
        Console.WriteLine($"5 - 3 = {calculator.Subtract(5, 3)}");
        Console.WriteLine($"4 * 3 = {calculator.Multiply(4, 3)}");
        Console.WriteLine($"10 / 2 = {calculator.Divide(10, 2)}");
        Console.WriteLine($"2^3 = {calculator.Power(2, 3)}");
        Console.WriteLine($"10 % 3 = {calculator.Modulo(10, 3)}");
        Console.WriteLine($"4 es par: {calculator.IsEven(4)}");
        Console.WriteLine($"7 es primo: {calculator.IsPrime(7)}");
        
        // Ejemplo 2: Validador de Email
        Console.WriteLine("\n2. VALIDADOR DE EMAIL:");
        var emailValidator = new UnitTesting.EmailValidatorUnitTests.EmailValidator();
        var testEmails = new[] { "test@example.com", "invalid-email", "" };
        foreach (var email in testEmails)
        {
            var isValid = emailValidator.IsValid(email);
            Console.WriteLine($"'{email}' es válido: {isValid}");
            
            if (!isValid)
            {
                var errors = emailValidator.GetValidationErrors(email);
                Console.WriteLine($"  Errores: {string.Join(", ", errors)}");
            }
        }
        
        // Ejemplo 3: Gestor de Inventario
        Console.WriteLine("\n3. GESTOR DE INVENTARIO:");
        var mockRepository = new UnitTesting.InventoryManagerUnitTests.MockProductRepository();
        var inventoryManager = new UnitTesting.InventoryManagerUnitTests.InventoryManager(mockRepository);
        
        // Agregar productos de prueba
        var product1 = new UnitTesting.InventoryManagerUnitTests.Product(0, "Laptop", 999.99m, 5, "Electronics");
        var product2 = new UnitTesting.InventoryManagerUnitTests.Product(0, "Mouse", 29.99m, 20, "Electronics");
        
        var createdProduct1 = inventoryManager.AddProductAsync(product1).Result;
        var createdProduct2 = inventoryManager.AddProductAsync(product2).Result;
        
        Console.WriteLine($"Producto 1: {createdProduct1.Name} - Stock: {createdProduct1.Stock}");
        Console.WriteLine($"Producto 2: {createdProduct2.Name} - Stock: {createdProduct2.Stock}");
        
        // Ejemplo 4: Validador de Contraseñas
        Console.WriteLine("\n4. VALIDADOR DE CONTRASEÑAS:");
        var passwordValidator = new UnitTesting.PasswordValidatorUnitTests.PasswordValidator();
        var testPasswords = new[] { "StrongPass123!", "weak", "password" };
        
        foreach (var password in testPasswords)
        {
            var isValid = passwordValidator.IsValid(password);
            var strength = passwordValidator.GetStrengthScore(password);
            Console.WriteLine($"'{password}' es válida: {isValid}, Fuerza: {strength}/6");
            
            if (!isValid)
            {
                var errors = passwordValidator.GetValidationErrors(password);
                Console.WriteLine($"  Errores: {string.Join(", ", errors)}");
            }
        }
        
        Console.WriteLine("\n✅ Testing Unitario comprendido!");
        Console.WriteLine("Recuerda: Las pruebas unitarias deben ser rápidas, aisladas y determinísticas.");
    }
}

// Programa principal
public class Program
{
    public static async Task Main()
    {
        UnitTestingDemonstration.DemonstrateUnitTesting();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Calculadora Avanzada
Implementa y prueba una calculadora que incluya:
- Funciones trigonométricas
- Logaritmos
- Raíz cuadrada
- Factorial

### Ejercicio 2: Validador de Formularios
Crea un validador completo que verifique:
- Nombres (solo letras y espacios)
- Edades (18-120 años)
- Números de teléfono
- Códigos postales

### Ejercicio 3: Gestor de Usuarios
Implementa un sistema de gestión de usuarios con:
- Validación de credenciales
- Gestión de roles
- Historial de actividad
- Validación de permisos

## 🔍 Puntos Clave

1. **Pruebas unitarias** verifican unidades individuales de código
2. **Patrón Arrange-Act-Assert** estructura las pruebas claramente
3. **Mocks y stubs** aíslan dependencias externas
4. **Teorías y datos de prueba** permiten múltiples escenarios
5. **Validación de excepciones** verifica manejo de errores
6. **Cobertura de código** asegura que todas las rutas sean probadas
7. **Pruebas determinísticas** dan el mismo resultado siempre
8. **Mantenibilidad** facilita cambios futuros

## 📚 Recursos Adicionales

- [xUnit Documentation](https://xunit.net/)
- [NUnit Documentation](https://docs.nunit.org/)
- [MSTest Documentation](https://docs.microsoft.com/en-us/visualstudio/test/unit-test-basics)
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

---

**🎯 ¡Has completado la Clase 3! Ahora comprendes el Testing Unitario**

**📚 [Siguiente: Clase 4 - Testing de Integración](clase_4_testing_integracion.md)**
