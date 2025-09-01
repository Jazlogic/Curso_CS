# 🚀 Clase 9: Testing Unitario

## 📋 Información de la Clase

- **Módulo**: Junior Level 3 - Programación Orientada a Objetos Avanzada
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 8 (Serialización Avanzada)

## 🎯 Objetivos de Aprendizaje

- Comprender los fundamentos del testing unitario
- Implementar tests unitarios con MSTest y NUnit
- Utilizar mocks y stubs para testing
- Aplicar técnicas de test-driven development (TDD)
- Crear suites de tests completas y mantenibles

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
| [Clase 7](clase_7_reflection_avanzada.md) | Reflection Avanzada | |
| [Clase 8](clase_8_serializacion_avanzada.md) | Serialización Avanzada | ← Anterior |
| **Clase 9** | **Testing Unitario** | ← Estás aquí |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final Integrador | Siguiente → |

**← [Volver al README del Módulo 3](../junior_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Fundamentos del Testing Unitario

El testing unitario verifica que cada unidad de código funcione correctamente de forma aislada.

```csharp
// Clases de ejemplo para testing
public interface ICalculadora
{
    int Sumar(int a, int b);
    int Restar(int a, int b);
    int Multiplicar(int a, int b);
    double Dividir(int a, int b);
}

public class Calculadora : ICalculadora
{
    public int Sumar(int a, int b) => a + b;
    
    public int Restar(int a, int b) => a - b;
    
    public int Multiplicar(int a, int b) => a * b;
    
    public double Dividir(int a, int b)
    {
        if (b == 0)
            throw new DivideByZeroException("No se puede dividir por cero");
        
        return (double)a / b;
    }
}

// Tests unitarios básicos con MSTest
[TestClass]
public class CalculadoraTests
{
    private ICalculadora _calculadora;
    
    [TestInitialize]
    public void Setup()
    {
        _calculadora = new Calculadora();
    }
    
    [TestMethod]
    public void Sumar_DosNumerosPositivos_RetornaSumaCorrecta()
    {
        // Arrange
        int a = 5;
        int b = 3;
        int resultadoEsperado = 8;
        
        // Act
        int resultado = _calculadora.Sumar(a, b);
        
        // Assert
        Assert.AreEqual(resultadoEsperado, resultado);
    }
    
    [TestMethod]
    public void Sumar_NumeroPositivoYNegativo_RetornaSumaCorrecta()
    {
        // Arrange
        int a = 10;
        int b = -3;
        int resultadoEsperado = 7;
        
        // Act
        int resultado = _calculadora.Sumar(a, b);
        
        // Assert
        Assert.AreEqual(resultadoEsperado, resultado);
    }
    
    [TestMethod]
    public void Restar_DosNumerosPositivos_RetornaRestaCorrecta()
    {
        // Arrange
        int a = 10;
        int b = 4;
        int resultadoEsperado = 6;
        
        // Act
        int resultado = _calculadora.Restar(a, b);
        
        // Assert
        Assert.AreEqual(resultadoEsperado, resultado);
    }
    
    [TestMethod]
    public void Multiplicar_DosNumeros_RetornaProductoCorrecto()
    {
        // Arrange
        int a = 6;
        int b = 7;
        int resultadoEsperado = 42;
        
        // Act
        int resultado = _calculadora.Multiplicar(a, b);
        
        // Assert
        Assert.AreEqual(resultadoEsperado, resultado);
    }
    
    [TestMethod]
    public void Dividir_DosNumerosPositivos_RetornaDivisionCorrecta()
    {
        // Arrange
        int a = 15;
        int b = 3;
        double resultadoEsperado = 5.0;
        
        // Act
        double resultado = _calculadora.Dividir(a, b);
        
        // Assert
        Assert.AreEqual(resultadoEsperado, resultado, 0.001);
    }
    
    [TestMethod]
    [ExpectedException(typeof(DivideByZeroException))]
    public void Dividir_PorCero_LanzaExcepcion()
    {
        // Arrange
        int a = 10;
        int b = 0;
        
        // Act
        _calculadora.Dividir(a, b);
        
        // Assert - Se espera que se lance la excepción
    }
    
    [TestMethod]
    public void Dividir_PorCero_LanzaExcepcionConMensajeCorrecto()
    {
        // Arrange
        int a = 10;
        int b = 0;
        
        // Act & Assert
        var excepcion = Assert.ThrowsException<DivideByZeroException>(
            () => _calculadora.Dividir(a, b));
        
        Assert.AreEqual("No se puede dividir por cero", excepcion.Message);
    }
}
```

### 2. Testing con Mocks y Stubs

Los mocks y stubs permiten simular dependencias externas para testing aislado.

```csharp
// Interfaces para testing
public interface IEmailService
{
    Task EnviarEmailAsync(string destinatario, string asunto, string mensaje);
    bool EsEmailValido(string email);
}

public interface IUsuarioRepository
{
    Task<Usuario> ObtenerPorIdAsync(int id);
    Task<bool> GuardarAsync(Usuario usuario);
    Task<bool> ExisteAsync(string email);
}

// Clase de negocio que usa las interfaces
public class UsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailService _emailService;
    
    public UsuarioService(IUsuarioRepository usuarioRepository, IEmailService emailService)
    {
        _usuarioRepository = usuarioRepository;
        _emailService = emailService;
    }
    
    public async Task<bool> CrearUsuarioAsync(Usuario usuario)
    {
        if (!_emailService.EsEmailValido(usuario.Email))
        {
            throw new ArgumentException("Email inválido");
        }
        
        if (await _usuarioRepository.ExisteAsync(usuario.Email))
        {
            throw new InvalidOperationException("El email ya está registrado");
        }
        
        var guardado = await _usuarioRepository.GuardarAsync(usuario);
        
        if (guardado)
        {
            await _emailService.EnviarEmailAsync(
                usuario.Email, 
                "Bienvenido", 
                $"Hola {usuario.Nombre}, tu cuenta ha sido creada exitosamente.");
        }
        
        return guardado;
    }
    
    public async Task<Usuario> ObtenerUsuarioAsync(int id)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(id);
        
        if (usuario == null)
        {
            throw new KeyNotFoundException($"Usuario con ID {id} no encontrado");
        }
        
        return usuario;
    }
}

// Tests usando mocks con Moq
[TestClass]
public class UsuarioServiceTests
{
    private Mock<IUsuarioRepository> _mockRepository;
    private Mock<IEmailService> _mockEmailService;
    private UsuarioService _usuarioService;
    
    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IUsuarioRepository>();
        _mockEmailService = new Mock<IEmailService>();
        _usuarioService = new UsuarioService(_mockRepository.Object, _mockEmailService.Object);
    }
    
    [TestMethod]
    public async Task CrearUsuario_EmailValidoYNoExiste_RetornaTrue()
    {
        // Arrange
        var usuario = new Usuario { Id = 1, Nombre = "Juan", Email = "juan@email.com" };
        
        _mockEmailService.Setup(x => x.EsEmailValido(usuario.Email)).Returns(true);
        _mockRepository.Setup(x => x.ExisteAsync(usuario.Email)).ReturnsAsync(false);
        _mockRepository.Setup(x => x.GuardarAsync(usuario)).ReturnsAsync(true);
        _mockEmailService.Setup(x => x.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var resultado = await _usuarioService.CrearUsuarioAsync(usuario);
        
        // Assert
        Assert.IsTrue(resultado);
        _mockRepository.Verify(x => x.GuardarAsync(usuario), Times.Once);
        _mockEmailService.Verify(x => x.EnviarEmailAsync(usuario.Email, "Bienvenido", It.IsAny<string>()), Times.Once);
    }
    
    [TestMethod]
    public async Task CrearUsuario_EmailInvalido_LanzaArgumentException()
    {
        // Arrange
        var usuario = new Usuario { Id = 1, Nombre = "Juan", Email = "email-invalido" };
        
        _mockEmailService.Setup(x => x.EsEmailValido(usuario.Email)).Returns(false);
        
        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _usuarioService.CrearUsuarioAsync(usuario));
        
        _mockRepository.Verify(x => x.GuardarAsync(It.IsAny<Usuario>()), Times.Never);
    }
    
    [TestMethod]
    public async Task CrearUsuario_EmailYaExiste_LanzaInvalidOperationException()
    {
        // Arrange
        var usuario = new Usuario { Id = 1, Nombre = "Juan", Email = "juan@email.com" };
        
        _mockEmailService.Setup(x => x.EsEmailValido(usuario.Email)).Returns(true);
        _mockRepository.Setup(x => x.ExisteAsync(usuario.Email)).ReturnsAsync(true);
        
        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => _usuarioService.CrearUsuarioAsync(usuario));
        
        _mockRepository.Verify(x => x.GuardarAsync(It.IsAny<Usuario>()), Times.Never);
    }
    
    [TestMethod]
    public async Task ObtenerUsuario_UsuarioExiste_RetornaUsuario()
    {
        // Arrange
        var usuario = new Usuario { Id = 1, Nombre = "Juan", Email = "juan@email.com" };
        _mockRepository.Setup(x => x.ObtenerPorIdAsync(1)).ReturnsAsync(usuario);
        
        // Act
        var resultado = await _usuarioService.ObtenerUsuarioAsync(1);
        
        // Assert
        Assert.IsNotNull(resultado);
        Assert.AreEqual(usuario.Id, resultado.Id);
        Assert.AreEqual(usuario.Nombre, resultado.Nombre);
        Assert.AreEqual(usuario.Email, resultado.Email);
    }
    
    [TestMethod]
    public async Task ObtenerUsuario_UsuarioNoExiste_LanzaKeyNotFoundException()
    {
        // Arrange
        _mockRepository.Setup(x => x.ObtenerPorIdAsync(999)).ReturnsAsync((Usuario)null);
        
        // Act & Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => _usuarioService.ObtenerUsuarioAsync(999));
    }
}

// Tests usando stubs (implementaciones simples)
[TestClass]
public class UsuarioServiceStubTests
{
    private UsuarioService _usuarioService;
    
    [TestInitialize]
    public void Setup()
    {
        var stubRepository = new StubUsuarioRepository();
        var stubEmailService = new StubEmailService();
        _usuarioService = new UsuarioService(stubRepository, stubEmailService);
    }
    
    [TestMethod]
    public async Task CrearUsuario_ConStubs_RetornaTrue()
    {
        // Arrange
        var usuario = new Usuario { Id = 1, Nombre = "Test", Email = "test@email.com" };
        
        // Act
        var resultado = await _usuarioService.CrearUsuarioAsync(usuario);
        
        // Assert
        Assert.IsTrue(resultado);
    }
}

// Implementaciones stub
public class StubUsuarioRepository : IUsuarioRepository
{
    private readonly List<Usuario> _usuarios = new();
    
    public Task<Usuario> ObtenerPorIdAsync(int id)
    {
        var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(usuario);
    }
    
    public Task<bool> GuardarAsync(Usuario usuario)
    {
        _usuarios.Add(usuario);
        return Task.FromResult(true);
    }
    
    public Task<bool> ExisteAsync(string email)
    {
        var existe = _usuarios.Any(u => u.Email == email);
        return Task.FromResult(existe);
    }
}

public class StubEmailService : IEmailService
{
    public Task EnviarEmailAsync(string destinatario, string asunto, string mensaje)
    {
        // Simula envío de email
        return Task.CompletedTask;
    }
    
    public bool EsEmailValido(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }
}
```

### 3. Test-Driven Development (TDD)

TDD es una metodología que escribe tests antes que el código de producción.

```csharp
// Ejemplo de TDD: Calculadora de estadísticas
[TestClass]
public class EstadisticasCalculatorTests
{
    private EstadisticasCalculator _calculator;
    
    [TestInitialize]
    public void Setup()
    {
        _calculator = new EstadisticasCalculator();
    }
    
    [TestMethod]
    public void CalcularPromedio_ListaVacia_LanzaArgumentException()
    {
        // Arrange
        var numeros = new List<double>();
        
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(
            () => _calculator.CalcularPromedio(numeros));
    }
    
    [TestMethod]
    public void CalcularPromedio_ListaConUnNumero_RetornaEseNumero()
    {
        // Arrange
        var numeros = new List<double> { 5.0 };
        double esperado = 5.0;
        
        // Act
        var resultado = _calculator.CalcularPromedio(numeros);
        
        // Assert
        Assert.AreEqual(esperado, resultado, 0.001);
    }
    
    [TestMethod]
    public void CalcularPromedio_ListaConVariosNumeros_RetornaPromedioCorrecto()
    {
        // Arrange
        var numeros = new List<double> { 1.0, 2.0, 3.0, 4.0, 5.0 };
        double esperado = 3.0;
        
        // Act
        var resultado = _calculator.CalcularPromedio(numeros);
        
        // Assert
        Assert.AreEqual(esperado, resultado, 0.001);
    }
    
    [TestMethod]
    public void CalcularMediana_ListaVacia_LanzaArgumentException()
    {
        // Arrange
        var numeros = new List<double>();
        
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(
            () => _calculator.CalcularMediana(numeros));
    }
    
    [TestMethod]
    public void CalcularMediana_ListaConUnNumero_RetornaEseNumero()
    {
        // Arrange
        var numeros = new List<double> { 7.0 };
        double esperado = 7.0;
        
        // Act
        var resultado = _calculator.CalcularMediana(numeros);
        
        // Assert
        Assert.AreEqual(esperado, resultado, 0.001);
    }
    
    [TestMethod]
    public void CalcularMediana_ListaConDosNumeros_RetornaPromedio()
    {
        // Arrange
        var numeros = new List<double> { 3.0, 7.0 };
        double esperado = 5.0;
        
        // Act
        var resultado = _calculator.CalcularMediana(numeros);
        
        // Assert
        Assert.AreEqual(esperado, resultado, 0.001);
    }
    
    [TestMethod]
    public void CalcularMediana_ListaConTresNumeros_RetornaNumeroDelMedio()
    {
        // Arrange
        var numeros = new List<double> { 1.0, 5.0, 9.0 };
        double esperado = 5.0;
        
        // Act
        var resultado = _calculator.CalcularMediana(numeros);
        
        // Assert
        Assert.AreEqual(esperado, resultado, 0.001);
    }
    
    [TestMethod]
    public void CalcularDesviacionEstandar_ListaVacia_LanzaArgumentException()
    {
        // Arrange
        var numeros = new List<double>();
        
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(
            () => _calculator.CalcularDesviacionEstandar(numeros));
    }
    
    [TestMethod]
    public void CalcularDesviacionEstandar_ListaConUnNumero_RetornaCero()
    {
        // Arrange
        var numeros = new List<double> { 10.0 };
        double esperado = 0.0;
        
        // Act
        var resultado = _calculator.CalcularDesviacionEstandar(numeros);
        
        // Assert
        Assert.AreEqual(esperado, resultado, 0.001);
    }
    
    [TestMethod]
    public void CalcularDesviacionEstandar_ListaConVariosNumeros_RetornaValorCorrecto()
    {
        // Arrange
        var numeros = new List<double> { 2.0, 4.0, 4.0, 4.0, 5.0, 5.0, 7.0, 9.0 };
        double esperado = 2.0; // Aproximadamente
        
        // Act
        var resultado = _calculator.CalcularDesviacionEstandar(numeros);
        
        // Assert
        Assert.AreEqual(esperado, resultado, 0.1);
    }
}

// Implementación de la calculadora de estadísticas (después de escribir los tests)
public class EstadisticasCalculator
{
    public double CalcularPromedio(List<double> numeros)
    {
        if (numeros == null || numeros.Count == 0)
            throw new ArgumentException("La lista no puede estar vacía");
        
        return numeros.Average();
    }
    
    public double CalcularMediana(List<double> numeros)
    {
        if (numeros == null || numeros.Count == 0)
            throw new ArgumentException("La lista no puede estar vacía");
        
        var ordenados = numeros.OrderBy(x => x).ToList();
        int n = ordenados.Count;
        
        if (n % 2 == 0)
        {
            // Número par de elementos
            return (ordenados[n / 2 - 1] + ordenados[n / 2]) / 2.0;
        }
        else
        {
            // Número impar de elementos
            return ordenados[n / 2];
        }
    }
    
    public double CalcularDesviacionEstandar(List<double> numeros)
    {
        if (numeros == null || numeros.Count == 0)
            throw new ArgumentException("La lista no puede estar vacía");
        
        if (numeros.Count == 1)
            return 0.0;
        
        var promedio = CalcularPromedio(numeros);
        var sumaCuadrados = numeros.Sum(x => Math.Pow(x - promedio, 2));
        var varianza = sumaCuadrados / (numeros.Count - 1);
        
        return Math.Sqrt(varianza);
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Tests para Calculadora Avanzada
Implementa tests unitarios para una calculadora que incluya funciones trigonométricas y logarítmicas.

### Ejercicio 2: Sistema de Validación con Mocks
Crea tests para un sistema de validación de formularios usando mocks para simular servicios externos.

### Ejercicio 3: API REST con TDD
Desarrolla una API REST simple siguiendo la metodología TDD.

## 🔍 Puntos Clave

1. **Tests unitarios** verifican el comportamiento de unidades de código aisladas
2. **Mocks y stubs** simulan dependencias externas para testing aislado
3. **TDD** escribe tests antes que el código de producción
4. **Arrange-Act-Assert** es el patrón estándar para estructurar tests
5. **Cobertura de código** mide qué porcentaje del código está cubierto por tests

## 📚 Recursos Adicionales

- [Unit Testing - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [MSTest Framework](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest)
- [Moq Framework](https://github.com/moq/moq4)

---

**🎯 ¡Has completado la Clase 9! Ahora dominas el testing unitario en C#**

**📚 [Siguiente: Clase 10 - Proyecto Final Integrador](clase_10_proyecto_final.md)**
