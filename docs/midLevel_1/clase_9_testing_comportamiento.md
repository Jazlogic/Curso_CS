# 🚀 Clase 9: Testing de Comportamiento (BDD)

## 📋 Información de la Clase

- **Módulo**: Mid Level 1 - Programación Avanzada y Patrones de Diseño
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 8 (Testing de Integración)

## 🎯 Objetivos de Aprendizaje

- Implementar BDD (Behavior Driven Development) con SpecFlow
- Crear escenarios de comportamiento legibles para stakeholders
- Implementar step definitions en C#
- Configurar tests de comportamiento para aplicaciones .NET
- Integrar BDD con CI/CD pipelines

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_patrones_diseno_intermedios.md) | Patrones de Diseño Intermedios | |
| [Clase 2](clase_2_programacion_asincrona_avanzada.md) | Programación Asíncrona Avanzada | |
| [Clase 3](clase_3_programacion_paralela.md) | Programación Paralela y TPL | |
| [Clase 4](clase_4_clean_architecture.md) | Clean Architecture | |
| [Clase 5](clase_5_dependency_injection.md) | Dependency Injection Avanzada | |
| [Clase 6](clase_6_logging_monitoreo.md) | Logging y Monitoreo | |
| [Clase 7](clase_7_refactoring_clean_code.md) | Refactoring y Clean Code | |
| [Clase 8](clase_8_testing_integracion.md) | Testing de Integración | ← Anterior |
| **Clase 9** | **Testing de Comportamiento (BDD)** | ← Estás aquí |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de E-commerce | Siguiente → |

**← [Volver al README del Módulo 4](../midLevel_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Introducción a BDD (Behavior Driven Development)

BDD es una metodología que fomenta la colaboración entre desarrolladores, QA y stakeholders no técnicos.

```csharp
// Configuración de SpecFlow para BDD
namespace ECommerceBDD
{
    using TechTalk.SpecFlow;
    using NUnit.Framework;
    using System.Threading.Tasks;
    
    // ===== ESCENARIOS DE COMPORTAMIENTO =====
    [Binding]
    public class ECommerceSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly ECommerceTestContext _testContext;
        
        public ECommerceSteps(ScenarioContext scenarioContext, ECommerceTestContext testContext)
        {
            _scenarioContext = scenarioContext;
            _testContext = testContext;
        }
        
        // ===== GIVEN - PREPARACIÓN DEL ESCENARIO =====
        [Given(@"que un usuario está registrado en el sistema")]
        public async Task GivenQueUnUsuarioEstaRegistradoEnElSistema()
        {
            // Preparar usuario de prueba
            var user = new User
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                IsActive = true
            };
            
            await _testContext.UserService.CreateUserAsync(user);
            _scenarioContext["CurrentUser"] = user;
            
            Console.WriteLine($"Usuario registrado: {user.Email}");
        }
        
        [Given(@"que el usuario tiene un carrito de compras vacío")]
        public async Task GivenQueElUsuarioTieneUnCarritoDeComprasVacio()
        {
            var user = _scenarioContext.Get<User>("CurrentUser");
            var cart = await _testContext.CartService.GetOrCreateCartAsync(user.Id);
            
            _scenarioContext["CurrentCart"] = cart;
            
            Console.WriteLine($"Carrito creado para usuario: {user.Email}");
        }
        
        [Given(@"que hay productos disponibles en el catálogo")]
        public async Task GivenQueHayProductosDisponiblesEnElCatalogo()
        {
            var products = new[]
            {
                new Product { Name = "Laptop", Price = 999.99m, Stock = 10 },
                new Product { Name = "Mouse", Price = 29.99m, Stock = 50 },
                new Product { Name = "Keyboard", Price = 79.99m, Stock = 25 }
            };
            
            foreach (var product in products)
            {
                await _testContext.ProductService.CreateProductAsync(product);
            }
            
            _scenarioContext["AvailableProducts"] = products;
            
            Console.WriteLine($"Productos agregados al catálogo: {products.Length}");
        }
        
        [Given(@"que el usuario tiene saldo suficiente en su cuenta")]
        public async Task GivenQueElUsuarioTieneSaldoSuficienteEnSuCuenta()
        {
            var user = _scenarioContext.Get<User>("CurrentUser");
            var account = new Account
            {
                UserId = user.Id,
                Balance = 2000.00m,
                Currency = "USD"
            };
            
            await _testContext.AccountService.CreateAccountAsync(account);
            _scenarioContext["UserAccount"] = account;
            
            Console.WriteLine($"Cuenta creada con saldo: {account.Balance} {account.Currency}");
        }
        
        // ===== WHEN - ACCIÓN DEL ESCENARIO =====
        [When(@"el usuario agrega un producto al carrito")]
        public async Task WhenElUsuarioAgregaUnProductoAlCarrito()
        {
            var cart = _scenarioContext.Get<Cart>("CurrentCart");
            var products = _scenarioContext.Get<Product[]>("AvailableProducts");
            var productToAdd = products[0]; // Laptop
            
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = productToAdd.Id,
                Quantity = 1,
                UnitPrice = productToAdd.Price
            };
            
            await _testContext.CartService.AddItemToCartAsync(cartItem);
            _scenarioContext["AddedProduct"] = productToAdd;
            
            Console.WriteLine($"Producto agregado al carrito: {productToAdd.Name}");
        }
        
        [When(@"el usuario procede al checkout")]
        public async Task WhenElUsuarioProcedeAlCheckout()
        {
            var cart = _scenarioContext.Get<Cart>("CurrentCart");
            var checkoutRequest = new CheckoutRequest
            {
                CartId = cart.Id,
                ShippingAddress = "123 Test Street, Test City",
                PaymentMethod = "CreditCard"
            };
            
            var checkoutResult = await _testContext.CheckoutService.ProcessCheckoutAsync(checkoutRequest);
            _scenarioContext["CheckoutResult"] = checkoutResult;
            
            Console.WriteLine($"Checkout procesado: {checkoutResult.OrderId}");
        }
        
        [When(@"el usuario confirma la orden")]
        public async Task WhenElUsuarioConfirmaLaOrden()
        {
            var checkoutResult = _scenarioContext.Get<CheckoutResult>("CheckoutResult");
            var order = await _testContext.OrderService.ConfirmOrderAsync(checkoutResult.OrderId);
            
            _scenarioContext["ConfirmedOrder"] = order;
            
            Console.WriteLine($"Orden confirmada: {order.Id}");
        }
        
        // ===== THEN - VERIFICACIÓN DEL RESULTADO =====
        [Then(@"el producto debe aparecer en el carrito")]
        public async Task EntoncesElProductoDebeAparecerEnElCarrito()
        {
            var cart = _scenarioContext.Get<Cart>("CurrentCart");
            var addedProduct = _scenarioContext.Get<Product>("AddedProduct");
            
            var cartItems = await _testContext.CartService.GetCartItemsAsync(cart.Id);
            
            Assert.That(cartItems, Is.Not.Empty);
            Assert.That(cartItems.Any(item => item.ProductId == addedProduct.Id), Is.True);
            
            var cartItem = cartItems.First(item => item.ProductId == addedProduct.Id);
            Assert.That(cartItem.Quantity, Is.EqualTo(1));
            Assert.That(cartItem.UnitPrice, Is.EqualTo(addedProduct.Price));
            
            Console.WriteLine($"Producto verificado en carrito: {addedProduct.Name}");
        }
        
        [Then(@"el total del carrito debe ser correcto")]
        public async Task EntoncesElTotalDelCarritoDebeSerCorrecto()
        {
            var cart = _scenarioContext.Get<Cart>("CurrentCart");
            var addedProduct = _scenarioContext.Get<Product>("AddedProduct");
            
            var cartTotal = await _testContext.CartService.GetCartTotalAsync(cart.Id);
            var expectedTotal = addedProduct.Price;
            
            Assert.That(cartTotal, Is.EqualTo(expectedTotal));
            
            Console.WriteLine($"Total del carrito verificado: {cartTotal}");
        }
        
        [Then(@"se debe crear una orden")]
        public void EntoncesSeDebeCrearUnaOrden()
        {
            var checkoutResult = _scenarioContext.Get<CheckoutResult>("CheckoutResult");
            
            Assert.That(checkoutResult, Is.Not.Null);
            Assert.That(checkoutResult.Success, Is.True);
            Assert.That(checkoutResult.OrderId, Is.Not.Null);
            
            Console.WriteLine($"Orden creada exitosamente: {checkoutResult.OrderId}");
        }
        
        [Then(@"el usuario debe recibir una confirmación por email")]
        public async Task EntoncesElUsuarioDebeRecibirUnaConfirmacionPorEmail()
        {
            var user = _scenarioContext.Get<User>("CurrentUser");
            var order = _scenarioContext.Get<Order>("ConfirmedOrder");
            
            var sentEmails = _testContext.EmailService.GetSentEmails();
            var confirmationEmail = sentEmails.FirstOrDefault(e => 
                e.To == user.Email && 
                e.Subject.Contains("Confirmación de Orden"));
            
            Assert.That(confirmationEmail, Is.Not.Null);
            Assert.That(confirmationEmail.Body, Contains.Substring(order.Id.ToString()));
            
            Console.WriteLine($"Email de confirmación enviado a: {user.Email}");
        }
        
        [Then(@"el inventario debe actualizarse")]
        public async Task EntoncesElInventarioDebeActualizarse()
        {
            var addedProduct = _scenarioContext.Get<Product>("AddedProduct");
            var updatedProduct = await _testContext.ProductService.GetProductAsync(addedProduct.Id);
            
            var expectedStock = addedProduct.Stock - 1; // Se vendió 1 unidad
            
            Assert.That(updatedProduct.Stock, Is.EqualTo(expectedStock));
            
            Console.WriteLine($"Inventario actualizado: {updatedProduct.Stock} unidades restantes");
        }
    }
    
    // ===== CONTEXTO DE PRUEBA =====
    public class ECommerceTestContext
    {
        public IUserService UserService { get; set; }
        public ICartService CartService { get; set; }
        public IProductService ProductService { get; set; }
        public IAccountService AccountService { get; set; }
        public ICheckoutService CheckoutService { get; set; }
        public IOrderService OrderService { get; set; }
        public IEmailService EmailService { get; set; }
        
        public ECommerceTestContext()
        {
            // Configurar servicios de prueba
            var services = new ServiceCollection();
            
            // Agregar servicios reales con mocks donde sea necesario
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ICheckoutService, CheckoutService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IEmailService, MockEmailService>();
            
            // Configurar base de datos de prueba
            services.AddDbContext<TestDbContext>(options =>
                options.UseInMemoryDatabase("BDDTestDb"));
            
            var serviceProvider = services.BuildServiceProvider();
            
            // Obtener instancias de servicios
            UserService = serviceProvider.GetRequiredService<IUserService>();
            CartService = serviceProvider.GetRequiredService<ICartService>();
            ProductService = serviceProvider.GetRequiredService<IProductService>();
            AccountService = serviceProvider.GetRequiredService<IAccountService>();
            CheckoutService = serviceProvider.GetRequiredService<ICheckoutService>();
            OrderService = serviceProvider.GetRequiredService<IOrderService>();
            EmailService = serviceProvider.GetRequiredService<IEmailService>();
        }
    }
    
    // ===== ESCENARIOS COMPLEJOS =====
    [Binding]
    public class AdvancedECommerceSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly ECommerceTestContext _testContext;
        
        public AdvancedECommerceSteps(ScenarioContext scenarioContext, ECommerceTestContext testContext)
        {
            _scenarioContext = scenarioContext;
            _testContext = testContext;
        }
        
        // ===== ESCENARIO: DESCUENTOS Y PROMOCIONES =====
        [Given(@"que hay una promoción activa del (.*)% de descuento")]
        public async Task GivenQueHayUnaPromocionActivaDelPorcentajeDeDescuento(int discountPercentage)
        {
            var promotion = new Promotion
            {
                Name = $"Descuento del {discountPercentage}%",
                DiscountPercentage = discountPercentage,
                IsActive = true,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo = DateTime.UtcNow.AddDays(30)
            };
            
            await _testContext.PromotionService.CreatePromotionAsync(promotion);
            _scenarioContext["ActivePromotion"] = promotion;
            
            Console.WriteLine($"Promoción creada: {promotion.Name}");
        }
        
        [When(@"el usuario aplica el código de promoción")]
        public async Task WhenElUsuarioAplicaElCodigoDePromocion()
        {
            var cart = _scenarioContext.Get<Cart>("CurrentCart");
            var promotion = _scenarioContext.Get<Promotion>("ActivePromotion");
            
            var applyPromotionRequest = new ApplyPromotionRequest
            {
                CartId = cart.Id,
                PromotionCode = promotion.Code
            };
            
            var result = await _testContext.CartService.ApplyPromotionAsync(applyPromotionRequest);
            _scenarioContext["PromotionResult"] = result;
            
            Console.WriteLine($"Promoción aplicada: {result.Success}");
        }
        
        [Then(@"el descuento debe aplicarse al total")]
        public async Task EntoncesElDescuentoDebeAplicarseAlTotal()
        {
            var cart = _scenarioContext.Get<Cart>("CurrentCart");
            var promotion = _scenarioContext.Get<Promotion>("ActivePromotion");
            var originalTotal = _scenarioContext.Get<decimal>("OriginalTotal");
            
            var discountedTotal = await _testContext.CartService.GetCartTotalAsync(cart.Id);
            var expectedDiscount = originalTotal * (promotion.DiscountPercentage / 100.0m);
            var expectedTotal = originalTotal - expectedDiscount;
            
            Assert.That(discountedTotal, Is.EqualTo(expectedTotal).Within(0.01m));
            
            Console.WriteLine($"Descuento aplicado: {expectedDiscount:C}");
        }
        
        // ===== ESCENARIO: MÚLTIPLES PRODUCTOS =====
        [When(@"el usuario agrega (.*) unidades del producto ""(.*)""")]
        public async Task WhenElUsuarioAgregaUnidadesDelProducto(int quantity, string productName)
        {
            var cart = _scenarioContext.Get<Cart>("CurrentCart");
            var products = _scenarioContext.Get<Product[]>("AvailableProducts");
            var product = products.First(p => p.Name == productName);
            
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = quantity,
                UnitPrice = product.Price
            };
            
            await _testContext.CartService.AddItemToCartAsync(cartItem);
            _scenarioContext["LastAddedItem"] = cartItem;
            
            Console.WriteLine($"Agregadas {quantity} unidades de {productName}");
        }
        
        [Then(@"el carrito debe contener (.*) productos diferentes")]
        public async Task EntoncesElCarritoDebeContenerProductosDiferentes(int expectedProductCount)
        {
            var cart = _scenarioContext.Get<Cart>("CurrentCart");
            var cartItems = await _testContext.CartService.GetCartItemsAsync(cart.Id);
            
            var uniqueProducts = cartItems.Select(item => item.ProductId).Distinct().Count();
            
            Assert.That(uniqueProducts, Is.EqualTo(expectedProductCount));
            
            Console.WriteLine($"Productos únicos en carrito: {uniqueProducts}");
        }
        
        // ===== ESCENARIO: VALIDACIONES DE NEGOCIO =====
        [Given(@"que el producto ""(.*)"" tiene solo (.*) unidades en stock")]
        public async Task GivenQueElProductoTieneSoloUnidadesEnStock(string productName, int stock)
        {
            var products = _scenarioContext.Get<Product[]>("AvailableProducts");
            var product = products.First(p => p.Name == productName);
            
            product.Stock = stock;
            await _testContext.ProductService.UpdateProductAsync(product);
            
            Console.WriteLine($"Stock actualizado para {productName}: {stock} unidades");
        }
        
        [When(@"el usuario intenta agregar (.*) unidades")]
        public async Task WhenElUsuarioIntentaAgregarUnidades(int quantity)
        {
            var cart = _scenarioContext.Get<Cart>("CurrentCart");
            var lastAddedItem = _scenarioContext.Get<CartItem>("LastAddedItem");
            
            try
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = lastAddedItem.ProductId,
                    Quantity = quantity,
                    UnitPrice = lastAddedItem.UnitPrice
                };
                
                await _testContext.CartService.AddItemToCartAsync(cartItem);
                _scenarioContext["AddItemResult"] = "Success";
            }
            catch (InvalidOperationException ex)
            {
                _scenarioContext["AddItemResult"] = "Failed";
                _scenarioContext["ErrorMessage"] = ex.Message;
            }
            
            Console.WriteLine($"Intento de agregar {quantity} unidades: {_scenarioContext["AddItemResult"]}");
        }
        
        [Then(@"debe recibir un error de stock insuficiente")]
        public void EntoncesDebeRecibirUnErrorDeStockInsuficiente()
        {
            var result = _scenarioContext.Get<string>("AddItemResult");
            var errorMessage = _scenarioContext.Get<string>("ErrorMessage");
            
            Assert.That(result, Is.EqualTo("Failed"));
            Assert.That(errorMessage, Contains.Substring("stock").IgnoreCase);
            
            Console.WriteLine($"Error recibido: {errorMessage}");
        }
    }
    
    // ===== CONFIGURACIÓN DE HOOKS =====
    [Binding]
    public class ECommerceHooks
    {
        private readonly ECommerceTestContext _testContext;
        
        public ECommerceHooks(ECommerceTestContext testContext)
        {
            _testContext = testContext;
        }
        
        [BeforeScenario]
        public async Task BeforeScenario()
        {
            Console.WriteLine("=== Iniciando nuevo escenario ===");
            
            // Limpiar base de datos antes de cada escenario
            await _testContext.CleanupDatabaseAsync();
        }
        
        [AfterScenario]
        public async Task AfterScenario()
        {
            Console.WriteLine("=== Finalizando escenario ===");
            
            // Limpiar recursos después de cada escenario
            await _testContext.CleanupDatabaseAsync();
        }
        
        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            Console.WriteLine("=== Iniciando suite de tests BDD ===");
        }
        
        [AfterTestRun]
        public static void AfterTestRun()
        {
            Console.WriteLine("=== Finalizando suite de tests BDD ===");
        }
    }
    
    // ===== ESCENARIOS EN GHERKIN =====
    /*
    Feature: Carrito de Compras
    
    Scenario: Usuario agrega producto al carrito
        Given que un usuario está registrado en el sistema
        And que el usuario tiene un carrito de compras vacío
        And que hay productos disponibles en el catálogo
        When el usuario agrega un producto al carrito
        Then el producto debe aparecer en el carrito
        And el total del carrito debe ser correcto
    
    Scenario: Usuario completa una compra
        Given que un usuario está registrado en el sistema
        And que el usuario tiene un carrito de compras vacío
        And que hay productos disponibles en el catálogo
        And que el usuario tiene saldo suficiente en su cuenta
        When el usuario agrega un producto al carrito
        And el usuario procede al checkout
        And el usuario confirma la orden
        Then se debe crear una orden
        And el usuario debe recibir una confirmación por email
        And el inventario debe actualizarse
    
    Scenario Outline: Aplicar descuentos
        Given que un usuario está registrado en el sistema
        And que el usuario tiene un carrito de compras vacío
        And que hay productos disponibles en el catálogo
        And que hay una promoción activa del <discount>% de descuento
        When el usuario agrega un producto al carrito
        And el usuario aplica el código de promoción
        Then el descuento debe aplicarse al total
        
        Examples:
            | discount |
            | 10       |
            | 20       |
            | 25       |
    */
}

// Uso de BDD con SpecFlow
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== BDD (Behavior Driven Development) con SpecFlow ===\n");
        
        Console.WriteLine("BDD permite:");
        Console.WriteLine("1. Escenarios legibles para stakeholders no técnicos");
        Console.WriteLine("2. Colaboración entre desarrolladores, QA y business");
        Console.WriteLine("3. Tests que documentan el comportamiento del sistema");
        Console.WriteLine("4. Escenarios reutilizables y mantenibles");
        
        Console.WriteLine("\nPara ejecutar los tests BDD:");
        Console.WriteLine("dotnet test --filter \"Category=BDD\"");
        Console.WriteLine("dotnet test --filter \"FullyQualifiedName~ECommerceBDD\"");
        
        Console.WriteLine("\nLos archivos .feature contienen los escenarios en Gherkin");
        Console.WriteLine("Los Step Definitions implementan la lógica de los pasos");
        Console.WriteLine("Los Hooks manejan la configuración y limpieza");
    }
}
