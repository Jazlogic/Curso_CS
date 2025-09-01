# 🚀 Clase 5: Principios SOLID

## 📋 Información de la Clase

- **Módulo**: Junior Level 3 - Programación Orientada a Objetos Avanzada
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 4 (Patrones de Diseño Básicos)

## 🎯 Objetivos de Aprendizaje

- Comprender los principios SOLID de diseño de software
- Aplicar el principio de Responsabilidad Única (SRP)
- Implementar el principio de Abierto/Cerrado (OCP)
- Utilizar el principio de Sustitución de Liskov (LSP)
- Aplicar los principios de Segregación de Interfaces (ISP) y Inversión de Dependencias (DIP)

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia_multiple.md) | Herencia Múltiple y Composición | |
| [Clase 2](clase_2_interfaces_avanzadas.md) | Interfaces Avanzadas | |
| [Clase 3](clase_3_polimorfismo_avanzado.md) | Polimorfismo Avanzado | |
| [Clase 4](clase_4_patrones_diseno.md) | Patrones de Diseño Básicos | ← Anterior |
| **Clase 5** | **Principios SOLID** | ← Estás aquí |
| [Clase 6](clase_6_arquitectura_modular.md) | Arquitectura Modular | Siguiente → |
| [Clase 7](clase_7_reflection_avanzada.md) | Reflection Avanzada | |
| [Clase 8](clase_8_serializacion_avanzada.md) | Serialización Avanzada | |
| [Clase 9](clase_9_testing_unitario.md) | Testing Unitario | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final Integrador | |

**← [Volver al README del Módulo 3](../junior_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Principio de Responsabilidad Única (SRP)

Una clase debe tener una sola razón para cambiar, es decir, una sola responsabilidad.

```csharp
// ❌ VIOLACIÓN del SRP - Múltiples responsabilidades
public class Usuario
{
    public string Nombre { get; set; }
    public string Email { get; set; }
    
    public void GuardarEnBaseDeDatos() { /* Lógica de persistencia */ }
    public void EnviarEmail() { /* Lógica de envío de email */ }
    public void ValidarDatos() { /* Lógica de validación */ }
    public void GenerarReporte() { /* Lógica de reportes */ }
}

// ✅ APLICACIÓN del SRP - Responsabilidades separadas
public class Usuario
{
    public string Nombre { get; set; }
    public string Email { get; set; }
}

public class UsuarioRepository
{
    public void Guardar(Usuario usuario) { /* Lógica de persistencia */ }
    public Usuario ObtenerPorId(int id) { /* Lógica de consulta */ }
}

public class EmailService
{
    public void EnviarEmail(string destinatario, string asunto, string mensaje) { /* Lógica de envío */ }
}

public class UsuarioValidator
{
    public bool EsValido(Usuario usuario) { /* Lógica de validación */ }
}

public class ReporteService
{
    public void GenerarReporte(Usuario usuario) { /* Lógica de reportes */ }
}
```

### 2. Principio de Abierto/Cerrado (OCP)

Las entidades de software deben estar abiertas para extensión pero cerradas para modificación.

```csharp
// ❌ VIOLACIÓN del OCP - Necesita modificación para agregar nuevos tipos
public class CalculadoraDescuento
{
    public decimal CalcularDescuento(string tipoCliente, decimal monto)
    {
        switch (tipoCliente)
        {
            case "Regular":
                return monto * 0.05m;
            case "Premium":
                return monto * 0.10m;
            case "VIP":
                return monto * 0.15m;
            default:
                return 0;
        }
    }
}

// ✅ APLICACIÓN del OCP - Extensible sin modificación
public abstract class EstrategiaDescuento
{
    public abstract decimal CalcularDescuento(decimal monto);
}

public class DescuentoRegular : EstrategiaDescuento
{
    public override decimal CalcularDescuento(decimal monto) => monto * 0.05m;
}

public class DescuentoPremium : EstrategiaDescuento
{
    public override decimal CalcularDescuento(decimal monto) => monto * 0.10m;
}

public class DescuentoVIP : EstrategiaDescuento
{
    public override decimal CalcularDescuento(decimal monto) => monto * 0.15m;
}

public class CalculadoraDescuento
{
    private readonly EstrategiaDescuento _estrategia;
    
    public CalculadoraDescuento(EstrategiaDescuento estrategia)
    {
        _estrategia = estrategia;
    }
    
    public decimal CalcularDescuento(decimal monto) => _estrategia.CalcularDescuento(monto);
}
```

### 3. Principio de Sustitución de Liskov (LSP)

Los objetos de una clase derivada deben poder sustituir objetos de la clase base sin afectar la funcionalidad del programa.

```csharp
// ❌ VIOLACIÓN del LSP - Comportamiento inesperado
public class Rectangulo
{
    public virtual int Ancho { get; set; }
    public virtual int Alto { get; set; }
    
    public virtual int CalcularArea() => Ancho * Alto;
}

public class Cuadrado : Rectangulo
{
    public override int Ancho
    {
        get => base.Ancho;
        set
        {
            base.Ancho = value;
            base.Alto = value; // Forza que alto = ancho
        }
    }
    
    public override int Alto
    {
        get => base.Alto;
        set
        {
            base.Alto = value;
            base.Ancho = value; // Forza que ancho = alto
        }
    }
}

// ✅ APLICACIÓN del LSP - Comportamiento consistente
public abstract class Forma
{
    public abstract int CalcularArea();
}

public class Rectangulo : Forma
{
    public int Ancho { get; set; }
    public int Alto { get; set; }
    
    public override int CalcularArea() => Ancho * Alto;
}

public class Cuadrado : Forma
{
    public int Lado { get; set; }
    
    public override int CalcularArea() => Lado * Lado;
}
```

### 4. Principio de Segregación de Interfaces (ISP)

Los clientes no deben verse forzados a depender de interfaces que no utilizan.

```csharp
// ❌ VIOLACIÓN del ISP - Interfaz monolítica
public interface IWorker
{
    void Trabajar();
    void Comer();
    void Dormir();
    void CobrarSalario();
    void TomarVacaciones();
}

// ✅ APLICACIÓN del ISP - Interfaces específicas
public interface ITrabajable
{
    void Trabajar();
}

public interface IComestible
{
    void Comer();
}

public interface IDormible
{
    void Dormir();
}

public interface IEmpleado : ITrabajable, IComestible, IDormible
{
    void CobrarSalario();
    void TomarVacaciones();
}

public interface IRobot : ITrabajable
{
    void RecargarBateria();
}
```

### 5. Principio de Inversión de Dependencias (DIP)

Los módulos de alto nivel no deben depender de módulos de bajo nivel. Ambos deben depender de abstracciones.

```csharp
// ❌ VIOLACIÓN del DIP - Dependencia directa
public class NotificacionService
{
    private readonly EmailService _emailService;
    
    public NotificacionService()
    {
        _emailService = new EmailService(); // Dependencia concreta
    }
    
    public void EnviarNotificacion(string mensaje)
    {
        _emailService.Enviar(mensaje);
    }
}

// ✅ APLICACIÓN del DIP - Dependencia de abstracciones
public interface INotificacionService
{
    void EnviarNotificacion(string mensaje);
}

public interface IEmailService
{
    void Enviar(string mensaje);
}

public class NotificacionService : INotificacionService
{
    private readonly IEmailService _emailService;
    
    public NotificacionService(IEmailService emailService) // Inyección de dependencia
    {
        _emailService = emailService;
    }
    
    public void EnviarNotificacion(string mensaje)
    {
        _emailService.Enviar(mensaje);
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Refactorizar Clase Violadora del SRP
Identifica y refactoriza una clase que viole el principio de responsabilidad única.

### Ejercicio 2: Implementar Estrategias de Pago
Crea un sistema de estrategias de pago que respete el principio abierto/cerrado.

### Ejercicio 3: Diseñar Jerarquía de Formas
Implementa una jerarquía de formas geométricas que respete el principio de sustitución de Liskov.

## 🔍 Puntos Clave

1. **SRP**: Una clase, una responsabilidad
2. **OCP**: Abierto para extensión, cerrado para modificación
3. **LSP**: Los subtipos deben ser sustituibles por sus tipos base
4. **ISP**: Interfaces pequeñas y específicas
5. **DIP**: Depender de abstracciones, no de implementaciones

## 📚 Recursos Adicionales

- [SOLID Principles - Robert C. Martin](https://en.wikipedia.org/wiki/SOLID)
- [Clean Code - Martin Fowler](https://martinfowler.com/)
- [Design Principles - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)

---

**🎯 ¡Has completado la Clase 5! Ahora entiendes los principios SOLID en C#**

**📚 [Siguiente: Clase 6 - Arquitectura Modular](clase_6_arquitectura_modular.md)**
