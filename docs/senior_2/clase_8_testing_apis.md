# 🚀 Clase 8: Testing de APIs

## 📋 Información de la Clase

- **Módulo**: Senior Level 2 - Testing y TDD
- **Duración**: 2 horas
- **Nivel**: Avanzado
- **Prerrequisitos**: Testing de Código Asíncrono (Clase 7)

## 🎯 Objetivos de Aprendizaje

- Implementar testing de APIs REST
- Probar endpoints HTTP con diferentes métodos
- Validar respuestas y códigos de estado
- Implementar testing de integración de APIs

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_fundamentos_testing.md) | Fundamentos de Testing | |
| [Clase 2](clase_2_desarrollo_dirigido_pruebas.md) | Desarrollo Dirigido por Pruebas (TDD) | |
| [Clase 3](clase_3_testing_unitario.md) | Testing Unitario | |
| [Clase 4](clase_4_testing_integracion.md) | Testing de Integración | |
| [Clase 5](clase_5_testing_comportamiento.md) | Testing de Comportamiento | |
| [Clase 6](clase_6_mocking_framworks.md) | Frameworks de Mocking | |
| [Clase 7](clase_7_testing_asincrono.md) | Testing de Código Asíncrono | ← Anterior |
| **Clase 8** | **Testing de APIs** | ← Estás aquí |
| [Clase 9](clase_9_testing_database.md) | Testing de Base de Datos | Siguiente → |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Testing | |

**← [Volver al README del Módulo 2](../senior_2/README.md)**

---

## 📚 Contenido Teórico

### 1. ¿Qué es el Testing de APIs?

El testing de APIs verifica que los endpoints HTTP funcionen correctamente, validando respuestas, códigos de estado, y el comportamiento de la API en diferentes escenarios.

### 2. Características del Testing de APIs

- **Testing de endpoints** HTTP (GET, POST, PUT, DELETE)
- **Validación de respuestas** y códigos de estado
- **Testing de autenticación** y autorización
- **Verificación de formato** de datos (JSON, XML)

```csharp
// ===== TESTING DE APIS - IMPLEMENTACIÓN COMPLETA =====
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ApiTesting
{
    // ===== MODELOS DE DOMINIO =====
    namespace Models
    {
        public class User
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? LastLoginAt { get; set; }
            public List<string> Roles { get; set; } = new List<string>();
        }
        
        public class CreateUserRequest
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
        
        public class UpdateUserRequest
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public bool IsActive { get; set; }
        }
        
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
        
        public class LoginResponse
        {
            public string Token { get; set; }
            public User User { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
        
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }
        
        public class PaginatedResponse<T>
        {
            public List<T> Items { get; set; }
            public int TotalCount { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
            public bool HasPreviousPage { get; set; }
            public bool HasNextPage { get; set; }
        }
        
        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string Category { get; set; }
            public bool IsAvailable => Stock > 0;
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
        
        public class CreateProductRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string Category { get; set; }
        }
        
        public class Order
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public List<OrderItem> Items { get; set; } = new List<OrderItem>();
            public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
        
        public class OrderItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice => Quantity * UnitPrice;
        }
        
        public class CreateOrderRequest
        {
            public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        }
    }
    
    // ===== INTERFACES DE SERVICIOS =====
    namespace Interfaces
    {
        public interface IUserService
        {
            Task<User> GetUserByIdAsync(int id);
            Task<IEnumerable<User>> GetAllUsersAsync();
            Task<User> CreateUserAsync(CreateUserRequest request);
            Task<User> UpdateUserAsync(int id, UpdateUserRequest request);
            Task<bool> DeleteUserAsync(int id);
            Task<LoginResponse> AuthenticateAsync(LoginRequest request);
            Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        }
        
        public interface IProductService
        {
            Task<Product> GetProductByIdAsync(int id);
            Task<PaginatedResponse<Product>> GetProductsAsync(int pageNumber, int pageSize, string category = null);
            Task<Product> CreateProductAsync(CreateProductRequest request);
            Task<Product> UpdateProductAsync(int id, CreateProductRequest request);
            Task<bool> DeleteProductAsync(int id);
            Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        }
        
        public interface IOrderService
        {
            Task<Order> GetOrderByIdAsync(int id);
            Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
            Task<Order> CreateOrderAsync(int userId, CreateOrderRequest request);
            Task<bool> UpdateOrderStatusAsync(int id, string status);
            Task<bool> CancelOrderAsync(int id);
        }
        
        public interface IAuthService
        {
            Task<string> GenerateTokenAsync(User user);
            Task<bool> ValidateTokenAsync(string token);
            Task<User> GetUserFromTokenAsync(string token);
        }
    }
    
    // ===== CONTROLADORES DE API =====
    namespace Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class UsersController : ControllerBase
        {
            private readonly IUserService _userService;
            private readonly IAuthService _authService;
            
            public UsersController(IUserService userService, IAuthService authService)
            {
                _userService = userService;
                _authService = authService;
            }
            
            [HttpGet]
            public async Task<ActionResult<ApiResponse<IEnumerable<User>>>> GetUsers()
            {
                try
                {
                    var users = await _userService.GetAllUsersAsync();
                    var response = new ApiResponse<IEnumerable<User>>
                    {
                        Success = true,
                        Data = users,
                        Message = "Users retrieved successfully"
                    };
                    
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    var response = new ApiResponse<IEnumerable<User>>
                    {
                        Success = false,
                        Message = "Error retrieving users",
                        Errors = new List<string> { ex.Message }
                    };
                    
                    return StatusCode(500, response);
                }
            }
            
            [HttpGet("{id}")]
            public async Task<ActionResult<ApiResponse<User>>> GetUser(int id)
            {
                try
                {
                    var user = await _userService.GetUserByIdAsync(id);
                    if (user == null)
                    {
                        var notFoundResponse = new ApiResponse<User>
                        {
                            Success = false,
                            Message = "User not found"
                        };
                        
                        return NotFound(notFoundResponse);
                    }
                    
                    var response = new ApiResponse<User>
                    {
                        Success = true,
                        Data = user,
                        Message = "User retrieved successfully"
                    };
                    
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    var response = new ApiResponse<User>
                    {
                        Success = false,
                        Message = "Error retrieving user",
                        Errors = new List<string> { ex.Message }
                    };
                    
                    return StatusCode(500, response);
                }
            }
            
            [HttpPost]
            public async Task<ActionResult<ApiResponse<User>>> CreateUser([FromBody] CreateUserRequest request)
            {
                try
                {
                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();
                        
                        var response = new ApiResponse<User>
                        {
                            Success = false,
                            Message = "Validation failed",
                            Errors = errors
                        };
                        
                        return BadRequest(response);
                    }
                    
                    var user = await _userService.CreateUserAsync(request);
                    var successResponse = new ApiResponse<User>
                    {
                        Success = true,
                        Data = user,
                        Message = "User created successfully"
                    };
                    
                    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, successResponse);
                }
                catch (ArgumentException ex)
                {
                    var response = new ApiResponse<User>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = new List<string> { ex.Message }
                    };
                    
                    return BadRequest(response);
                }
                catch (Exception ex)
                {
                    var response = new ApiResponse<User>
                    {
                        Success = false,
                        Message = "Error creating user",
                        Errors = new List<string> { ex.Message }
                    };
                    
                    return StatusCode(500, response);
                }
            }
            
            [HttpPut("{id}")]
            public async Task<ActionResult<ApiResponse<User>>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
            {
                try
                {
                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();
                        
                        var response = new ApiResponse<User>
                        {
                            Success = false,
                            Message = "Validation failed",
                            Errors = errors
                        };
                        
                        return BadRequest(response);
                    }
                    
                    var user = await _userService.UpdateUserAsync(id, request);
                    if (user == null)
                    {
                        var notFoundResponse = new ApiResponse<User>
                        {
                            Success = false,
                            Message = "User not found"
                        };
                        
                        return NotFound(notFoundResponse);
                    }
                    
                    var successResponse = new ApiResponse<User>
                    {
                        Success = true,
                        Data = user,
                        Message = "User updated successfully"
                    };
                    
                    return Ok(successResponse);
                }
                catch (Exception ex)
                {
                    var response = new ApiResponse<User>
                    {
                        Success = false,
                        Message = "Error updating user",
                        Errors = new List<string> { ex.Message }
                    };
                    
                    return StatusCode(500, response);
                }
            }
            
            [HttpDelete("{id}")]
            public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(int id)
            {
                try
                {
                    var result = await _userService.DeleteUserAsync(id);
                    if (!result)
                    {
                        var notFoundResponse = new ApiResponse<bool>
                        {
                            Success = false,
                            Message = "User not found"
                        };
                        
                        return NotFound(notFoundResponse);
                    }
                    
                    var successResponse = new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "User deleted successfully"
                    };
                    
                    return Ok(successResponse);
                }
                catch (Exception ex)
                {
                    var response = new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Error deleting user",
                        Errors = new List<string> { ex.Message }
                    };
                    
                    return StatusCode(500, response);
                }
            }
            
            [HttpPost("login")]
            public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
            {
                try
                {
                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();
                        
                        var response = new ApiResponse<LoginResponse>
                        {
                            Success = false,
                            Message = "Validation failed",
                            Errors = errors
                        };
                        
                        return BadRequest(response);
                    }
                    
                    var loginResponse = await _userService.AuthenticateAsync(request);
                    if (loginResponse == null)
                    {
                        var unauthorizedResponse = new ApiResponse<LoginResponse>
                        {
                            Success = false,
                            Message = "Invalid credentials"
                        };
                        
                        return Unauthorized(unauthorizedResponse);
                    }
                    
                    var successResponse = new ApiResponse<LoginResponse>
                    {
                        Success = true,
                        Data = loginResponse,
                        Message = "Login successful"
                    };
                    
                    return Ok(successResponse);
                }
                catch (Exception ex)
                {
                    var response = new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Error during login",
                        Errors = new List<string> { ex.Message }
                    };
                    
                    return StatusCode(500, response);
                }
            }
        }
        
        [ApiController]
        [Route("api/[controller]")]
        public class ProductsController : ControllerBase
        {
            private readonly IProductService _productService;
            
            public ProductsController(IProductService productService)
            {
                _productService = productService;
            }
            
            [HttpGet]
            public async Task<ActionResult<ApiResponse<PaginatedResponse<Product>>>> GetProducts(
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string category = null)
            {
                try
                {
                    var products = await _productService.GetProductsAsync(pageNumber, pageSize, category);
                    var response = new ApiResponse<PaginatedResponse<Product>>
                    {
                        Success = true,
                        Data = products,
                        Message = "Products retrieved successfully"
                    };
                    
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    var response = new ApiResponse<PaginatedResponse<Product>>
                    {
                        Success = false,
                        Message = "Error retrieving products",
                        Errors = new List<string> { ex.Message }
                    };
                    
                    return StatusCode(500, response);
                }
            }
            
            [HttpGet("{id}")]
            public async Task<ActionResult<ApiResponse<Product>>> GetProduct(int id)
            {
                try
                {
                    var product = await _productService.GetProductByIdAsync(id);
                    if (product == null)
                    {
                        var notFoundResponse = new ApiResponse<Product>
                        {
                            Success = false,
                            Message = "Product not found"
                        };
                        
                        return NotFound(notFoundResponse);
                    }
                    
                    var response = new ApiResponse<Product>
                    {
                        Success = true,
                        Data = product,
                        Message = "Product retrieved successfully"
                    };
                    
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    var response = new ApiResponse<Product>
                    {
                        Success = false,
                        Message = "Error retrieving product",
                        Errors = new List<string> { ex.Message }
                    };
                    
                    return StatusCode(500, response);
                }
            }
            
            [HttpPost]
            public async Task<ActionResult<ApiResponse<Product>>> CreateProduct([FromBody] CreateProductRequest request)
            {
                try
                {
                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();
                        
                        var response = new ApiResponse<Product>
                        {
                            Success = false,
                            Message = "Validation failed",
                            Errors = errors
                        };
                        
                        return BadRequest(response);
                    }
                    
                    var product = await _productService.CreateProductAsync(request);
                    var successResponse = new ApiResponse<Product>
                    {
                        Success = true,
                        Data = product,
                        Message = "Product created successfully"
                    };
                    
                    return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, successResponse);
                }
                catch (ArgumentException ex)
                {
                    var response = new ApiResponse<Product>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = new List<string> { ex.Message }
                    };
                    
                    return BadRequest(response);
                }
                catch (Exception ex)
                {
                    var response = new ApiResponse<Product>
                    {
                        Success = false,
                        Message = "Error creating product",
                        Errors = new List<string> { ex.Message }
                    };
                    
                    return StatusCode(500, response);
                }
            }
            
            [HttpGet("search")]
            public async Task<ActionResult<ApiResponse<IEnumerable<Product>>>> SearchProducts([FromQuery] string q)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(q))
                    {
                        var response = new ApiResponse<IEnumerable<Product>>
                        {
                            Success = false,
                            Message = "Search term is required"
                        };
                        
                        return BadRequest(response);
                    }
                    
                    var products = await _productService.SearchProductsAsync(q);
                    var successResponse = new ApiResponse<IEnumerable<Product>>
                    {
                        Success = true,
                        Data = products,
                        Message = "Search completed successfully"
                    };
                    
                    return Ok(successResponse);
                }
                catch (Exception ex)
                {
                    var response = new ApiResponse<IEnumerable<Product>>
                    {
                        Success = false,
                        Message = "Error during search",
                        Errors = new List<string> { ex.Message }
                    };
                    
                    return StatusCode(500, response);
                }
            }
        }
    }
    
    // ===== TESTING DE APIS CON WEBAPPLICATIONFACTORY =====
    namespace WebApplicationFactoryTests
    {
        public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
        {
            private readonly WebApplicationFactory<Program> _factory;
            private readonly HttpClient _client;
            
            public UsersControllerTests(WebApplicationFactory<Program> factory)
            {
                _factory = factory;
                _client = _factory.CreateClient();
            }
            
            [Fact]
            public async Task GetUsers_ShouldReturnOkWithUsers()
            {
                // Act
                var response = await _client.GetAsync("/api/users");
                
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<User>>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Assert.NotNull(apiResponse);
                Assert.True(apiResponse.Success);
                Assert.NotNull(apiResponse.Data);
            }
            
            [Fact]
            public async Task GetUser_WithValidId_ShouldReturnOkWithUser()
            {
                // Arrange
                var userId = 1;
                
                // Act
                var response = await _client.GetAsync($"/api/users/{userId}");
                
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<User>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Assert.NotNull(apiResponse);
                Assert.True(apiResponse.Success);
                Assert.NotNull(apiResponse.Data);
                Assert.Equal(userId, apiResponse.Data.Id);
            }
            
            [Fact]
            public async Task GetUser_WithInvalidId_ShouldReturnNotFound()
            {
                // Arrange
                var invalidUserId = 999;
                
                // Act
                var response = await _client.GetAsync($"/api/users/{invalidUserId}");
                
                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<User>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Assert.NotNull(apiResponse);
                Assert.False(apiResponse.Success);
                Assert.Contains("not found", apiResponse.Message.ToLower());
            }
            
            [Fact]
            public async Task CreateUser_WithValidData_ShouldReturnCreated()
            {
                // Arrange
                var createRequest = new CreateUserRequest
                {
                    Username = "testuser",
                    Email = "test@example.com",
                    Password = "password123",
                    FirstName = "Test",
                    LastName = "User"
                };
                
                var json = JsonSerializer.Serialize(createRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Act
                var response = await _client.PostAsync("/api/users", content);
                
                // Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<User>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Assert.NotNull(apiResponse);
                Assert.True(apiResponse.Success);
                Assert.NotNull(apiResponse.Data);
                Assert.Equal(createRequest.Username, apiResponse.Data.Username);
                Assert.Equal(createRequest.Email, apiResponse.Data.Email);
                
                // Verificar que se incluye el header Location
                Assert.True(response.Headers.Contains("Location"));
            }
            
            [Fact]
            public async Task CreateUser_WithInvalidData_ShouldReturnBadRequest()
            {
                // Arrange
                var invalidRequest = new CreateUserRequest
                {
                    Username = "", // Inválido
                    Email = "invalid-email", // Inválido
                    Password = "123" // Muy corto
                };
                
                var json = JsonSerializer.Serialize(invalidRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Act
                var response = await _client.PostAsync("/api/users", content);
                
                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<User>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Assert.NotNull(apiResponse);
                Assert.False(apiResponse.Success);
                Assert.Contains("validation failed", apiResponse.Message.ToLower());
                Assert.NotNull(apiResponse.Errors);
                Assert.True(apiResponse.Errors.Count > 0);
            }
            
            [Fact]
            public async Task UpdateUser_WithValidData_ShouldReturnOk()
            {
                // Arrange
                var userId = 1;
                var updateRequest = new UpdateUserRequest
                {
                    FirstName = "Updated",
                    LastName = "Name",
                    IsActive = false
                };
                
                var json = JsonSerializer.Serialize(updateRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Act
                var response = await _client.PutAsync($"/api/users/{userId}", content);
                
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<User>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Assert.NotNull(apiResponse);
                Assert.True(apiResponse.Success);
                Assert.NotNull(apiResponse.Data);
                Assert.Equal(updateRequest.FirstName, apiResponse.Data.FirstName);
                Assert.Equal(updateRequest.LastName, apiResponse.Data.LastName);
                Assert.Equal(updateRequest.IsActive, apiResponse.Data.IsActive);
            }
            
            [Fact]
            public async Task DeleteUser_WithValidId_ShouldReturnOk()
            {
                // Arrange
                var userId = 1;
                
                // Act
                var response = await _client.DeleteAsync($"/api/users/{userId}");
                
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<bool>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Assert.NotNull(apiResponse);
                Assert.True(apiResponse.Success);
                Assert.True(apiResponse.Data);
            }
            
            [Fact]
            public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
            {
                // Arrange
                var loginRequest = new LoginRequest
                {
                    Username = "testuser",
                    Password = "password123"
                };
                
                var json = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Act
                var response = await _client.PostAsync("/api/users/login", content);
                
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Assert.NotNull(apiResponse);
                Assert.True(apiResponse.Success);
                Assert.NotNull(apiResponse.Data);
                Assert.NotNull(apiResponse.Data.Token);
                Assert.NotNull(apiResponse.Data.User);
            }
            
            [Fact]
            public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
            {
                // Arrange
                var invalidLoginRequest = new LoginRequest
                {
                    Username = "invaliduser",
                    Password = "wrongpassword"
                };
                
                var json = JsonSerializer.Serialize(invalidLoginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Act
                var response = await _client.PostAsync("/api/users/login", content);
                
                // Assert
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Assert.NotNull(apiResponse);
                Assert.False(apiResponse.Success);
                Assert.Contains("invalid credentials", apiResponse.Message.ToLower());
            }
        }
        
        public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
        {
            private readonly WebApplicationFactory<Program> _factory;
            private readonly HttpClient _client;
            
            public ProductsControllerTests(WebApplicationFactory<Program> factory)
            {
                _factory = factory;
                _client = _factory.CreateClient();
            }
            
            [Fact]
            public async Task GetProducts_ShouldReturnOkWithPaginatedProducts()
            {
                // Act
                var response = await _client.GetAsync("/api/products?pageNumber=1&pageSize=5");
                
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedResponse<Product>>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Assert.NotNull(apiResponse);
                Assert.True(apiResponse.Success);
                Assert.NotNull(apiResponse.Data);
                Assert.NotNull(apiResponse.Data.Items);
                Assert.True(apiResponse.Data.Items.Count <= 5);
                Assert.True(apiResponse.Data.PageNumber == 1);
                Assert.True(apiResponse.Data.PageSize == 5);
            }
            
            [Fact]
            public async Task GetProducts_WithCategoryFilter_ShouldReturnFilteredProducts()
            {
                // Arrange
                var category = "Electronics";
                
                // Act
                var response = await _client.GetAsync($"/api/products?category={category}");
                
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedResponse<Product>>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Assert.NotNull(apiResponse);
                Assert.True(apiResponse.Success);
                Assert.NotNull(apiResponse.Data);
                Assert.NotNull(apiResponse.Data.Items);
                
                // Verificar que todos los productos pertenecen a la categoría especificada
                Assert.All(apiResponse.Data.Items, product => Assert.Equal(category, product.Category));
            }
            
            [Fact]
            public async Task SearchProducts_WithValidQuery_ShouldReturnMatchingProducts()
            {
                // Arrange
                var searchTerm = "laptop";
                
                // Act
                var response = await _client.GetAsync($"/api/products/search?q={searchTerm}");
                
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<Product>>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Assert.NotNull(apiResponse);
                Assert.True(apiResponse.Success);
                Assert.NotNull(apiResponse.Data);
                
                // Verificar que al menos un producto contiene el término de búsqueda
                var hasMatchingProduct = apiResponse.Data.Any(p => 
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                
                Assert.True(hasMatchingProduct);
            }
            
            [Fact]
            public async Task SearchProducts_WithEmptyQuery_ShouldReturnBadRequest()
            {
                // Act
                var response = await _client.GetAsync("/api/products/search?q=");
                
                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<Product>>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Assert.NotNull(apiResponse);
                Assert.False(apiResponse.Success);
                Assert.Contains("search term is required", apiResponse.Message.ToLower());
            }
        }
    }
    
    // ===== TESTING DE APIS CON HTTPCLIENT =====
    namespace HttpClientTests
    {
        public class ApiClientTests
        {
            private readonly HttpClient _httpClient;
            private readonly string _baseUrl = "https://jsonplaceholder.typicode.com"; // API de prueba pública
            
            public ApiClientTests()
            {
                _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_baseUrl);
            }
            
            [Fact]
            public async Task GetPosts_ShouldReturnOkWithPosts()
            {
                // Act
                var response = await _httpClient.GetAsync("/posts");
                
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
                
                var content = await response.Content.ReadAsStringAsync();
                Assert.NotEmpty(content);
                
                // Verificar que es un array JSON válido
                var posts = JsonSerializer.Deserialize<List<dynamic>>(content);
                Assert.NotNull(posts);
                Assert.True(posts.Count > 0);
            }
            
            [Fact]
            public async Task GetPost_WithValidId_ShouldReturnOkWithPost()
            {
                // Arrange
                var postId = 1;
                
                // Act
                var response = await _httpClient.GetAsync($"/posts/{postId}");
                
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
                var content = await response.Content.ReadAsStringAsync();
                var post = JsonSerializer.Deserialize<dynamic>(content);
                
                Assert.NotNull(post);
                Assert.Equal(postId, post.GetProperty("id").GetInt32());
            }
            
            [Fact]
            public async Task GetPost_WithInvalidId_ShouldReturnNotFound()
            {
                // Arrange
                var invalidPostId = 999;
                
                // Act
                var response = await _httpClient.GetAsync($"/posts/{invalidPostId}");
                
                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
            
            [Fact]
            public async Task CreatePost_WithValidData_ShouldReturnCreated()
            {
                // Arrange
                var postData = new
                {
                    title = "Test Post",
                    body = "This is a test post",
                    userId = 1
                };
                
                var json = JsonSerializer.Serialize(postData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Act
                var response = await _httpClient.PostAsync("/posts", content);
                
                // Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var createdPost = JsonSerializer.Deserialize<dynamic>(responseContent);
                
                Assert.NotNull(createdPost);
                Assert.Equal(postData.title, createdPost.GetProperty("title").GetString());
                Assert.Equal(postData.body, createdPost.GetProperty("body").GetString());
                Assert.Equal(postData.userId, createdPost.GetProperty("userId").GetInt32());
                Assert.True(createdPost.GetProperty("id").GetInt32() > 0);
            }
            
            [Fact]
            public async Task UpdatePost_WithValidData_ShouldReturnOk()
            {
                // Arrange
                var postId = 1;
                var updateData = new
                {
                    id = postId,
                    title = "Updated Post",
                    body = "This post has been updated",
                    userId = 1
                };
                
                var json = JsonSerializer.Serialize(updateData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Act
                var response = await _httpClient.PutAsync($"/posts/{postId}", content);
                
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var updatedPost = JsonSerializer.Deserialize<dynamic>(responseContent);
                
                Assert.NotNull(updatedPost);
                Assert.Equal(updateData.title, updatedPost.GetProperty("title").GetString());
                Assert.Equal(updateData.body, updatedPost.GetProperty("body").GetString());
            }
            
            [Fact]
            public async Task DeletePost_WithValidId_ShouldReturnOk()
            {
                // Arrange
                var postId = 1;
                
                // Act
                var response = await _httpClient.DeleteAsync($"/posts/{postId}");
                
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}

// ===== DEMOSTRACIÓN DE TESTING DE APIS =====
public class ApiTestingDemonstration
{
    public static async Task DemonstrateApiTesting()
    {
        Console.WriteLine("=== Testing de APIs - Clase 8 ===\n");
        
        Console.WriteLine("1. CREANDO CLIENTE HTTP:");
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
        
        Console.WriteLine("✅ Cliente HTTP creado");
        
        Console.WriteLine("\n2. PROBANDO ENDPOINT GET:");
        try
        {
            var response = await httpClient.GetAsync("/posts/1");
            Console.WriteLine($"✅ GET /posts/1 - Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✅ Contenido recibido: {content.Length} caracteres");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en GET: {ex.Message}");
        }
        
        Console.WriteLine("\n3. PROBANDO ENDPOINT POST:");
        try
        {
            var postData = new
            {
                title = "Test Post from C#",
                body = "This is a test post created during API testing demonstration",
                userId = 1
            };
            
            var json = System.Text.Json.JsonSerializer.Serialize(postData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var postResponse = await httpClient.PostAsync("/posts", content);
            Console.WriteLine($"✅ POST /posts - Status: {postResponse.StatusCode}");
            
            if (postResponse.IsSuccessStatusCode)
            {
                var responseContent = await postResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"✅ Post creado exitosamente");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en POST: {ex.Message}");
        }
        
        Console.WriteLine("\n4. PROBANDO ENDPOINT PUT:");
        try
        {
            var updateData = new
            {
                id = 1,
                title = "Updated Post",
                body = "This post has been updated during testing",
                userId = 1
            };
            
            var json = System.Text.Json.JsonSerializer.Serialize(updateData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var putResponse = await httpClient.PutAsync("/posts/1", content);
            Console.WriteLine($"✅ PUT /posts/1 - Status: {putResponse.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en PUT: {ex.Message}");
        }
        
        Console.WriteLine("\n5. PROBANDO ENDPOINT DELETE:");
        try
        {
            var deleteResponse = await httpClient.DeleteAsync("/posts/1");
            Console.WriteLine($"✅ DELETE /posts/1 - Status: {deleteResponse.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en DELETE: {ex.Message}");
        }
        
        Console.WriteLine("\n✅ Testing de APIs demostrado!");
        Console.WriteLine("El testing de APIs permite verificar el comportamiento de endpoints HTTP, validar respuestas y probar diferentes escenarios.");
    }
}

// Programa principal
public class Program
{
    public static async Task Main()
    {
        await ApiTestingDemonstration.DemonstrateApiTesting();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Testing de Endpoints CRUD
Implementa pruebas para:
- Operaciones de creación, lectura, actualización y eliminación
- Validación de datos de entrada
- Manejo de errores y códigos de estado

### Ejercicio 2: Testing de Autenticación
Crea pruebas que verifiquen:
- Endpoints protegidos
- Validación de tokens
- Manejo de credenciales inválidas

### Ejercicio 3: Testing de Validación
Implementa testing para:
- Modelos de datos inválidos
- Campos requeridos faltantes
- Formato de datos incorrecto

## 🔍 Puntos Clave

1. **WebApplicationFactory** permite testing de aplicaciones ASP.NET Core
2. **HttpClient** se usa para testing de APIs externas
3. **Validación de respuestas** incluye códigos de estado y contenido
4. **Testing de endpoints** cubre todos los métodos HTTP
5. **Manejo de errores** verifica respuestas apropiadas
6. **Validación de datos** asegura formato correcto
7. **Testing de autenticación** verifica seguridad
8. **Serialización JSON** permite verificación de contenido

## 📚 Recursos Adicionales

- [ASP.NET Core Testing](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [WebApplicationFactory](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [HTTP Client Testing](https://docs.microsoft.com/en-us/dotnet/core/extensions/http-client-factory)

---

**🎯 ¡Has completado la Clase 8! Ahora comprendes el Testing de APIs**

**📚 [Siguiente: Clase 9 - Testing de Base de Datos](clase_9_testing_database.md)**
