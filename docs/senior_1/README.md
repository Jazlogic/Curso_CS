# 🎯 Senior Level 1: Patrones de Diseño y Principios SOLID

## 📚 Descripción

En este nivel aprenderás los patrones de diseño más importantes y los principios SOLID que son fundamentales para crear software de alta calidad, mantenible y escalable. Estos conceptos son esenciales para cualquier desarrollador senior.

## 🎯 Objetivos de Aprendizaje

- Entender y aplicar los principios SOLID
- Implementar patrones de diseño creacionales, estructurales y de comportamiento
- Crear arquitecturas de software robustas y mantenibles
- Aplicar patrones de diseño en situaciones reales
- Entender cuándo y cómo usar cada patrón
- Crear código que sea fácil de testear y extender

## 📖 Contenido Teórico

### 1. Principios SOLID

#### Single Responsibility Principle (SRP)
Una clase debe tener una sola razón para cambiar, es decir, una sola responsabilidad.

```csharp
// ❌ Mal: Múltiples responsabilidades
public class Usuario
{
    public void GuardarUsuario() { /* Lógica de persistencia */ }
    public void EnviarEmail() { /* Lógica de email */ }
    public void ValidarDatos() { /* Lógica de validación */ }
    public void GenerarReporte() { /* Lógica de reportes */ }
}

// ✅ Bien: Responsabilidad única
public class Usuario
{
    public string Nombre { get; set; }
    public string Email { get; set; }
}

public class UsuarioRepository
{
    public void Guardar(Usuario usuario) { /* Solo persistencia */ }
}

public class EmailService
{
    public void EnviarEmail(string email, string mensaje) { /* Solo email */ }
}

public class UsuarioValidator
{
    public bool EsValido(Usuario usuario) { /* Solo validación */ }
}
```

#### Open/Closed Principle (OCP)
Las entidades de software deben estar abiertas para extensión pero cerradas para modificación.

```csharp
// ❌ Mal: Necesita modificación para agregar nuevas formas
public class CalculadoraArea
{
    public double CalcularArea(string tipo, double[] parametros)
    {
        switch (tipo)
        {
            case "rectangulo":
                return parametros[0] * parametros[1];
            case "circulo":
                return Math.PI * parametros[0] * parametros[0];
            default:
                throw new ArgumentException("Tipo no soportado");
        }
    }
}

// ✅ Bien: Abierto para extensión
public interface IForma
{
    double CalcularArea();
}

public class Rectangulo : IForma
{
    public double Base { get; set; }
    public double Altura { get; set; }
    
    public double CalcularArea() => Base * Altura;
}

public class Circulo : IForma
{
    public double Radio { get; set; }
    
    public double CalcularArea() => Math.PI * Radio * Radio;
}

public class CalculadoraArea
{
    public double CalcularArea(IForma forma)
    {
        return forma.CalcularArea();
    }
}
```

#### Liskov Substitution Principle (LSP)
Los objetos de una clase derivada deben poder sustituir a los objetos de la clase base sin afectar la funcionalidad del programa.

```csharp
// ❌ Mal: Viola LSP
public class Rectangulo
{
    public virtual double Base { get; set; }
    public virtual double Altura { get; set; }
    
    public virtual double CalcularArea() => Base * Altura;
}

public class Cuadrado : Rectangulo
{
    public override double Base
    {
        get => base.Base;
        set
        {
            base.Base = value;
            base.Altura = value; // Cambia el comportamiento esperado
        }
    }
    
    public override double Altura
    {
        get => base.Altura;
        set
        {
            base.Altura = value;
            base.Base = value; // Cambia el comportamiento esperado
        }
    }
}

// ✅ Bien: Respeta LSP
public interface IForma
{
    double CalcularArea();
}

public class Rectangulo : IForma
{
    public double Base { get; set; }
    public double Altura { get; set; }
    
    public double CalcularArea() => Base * Altura;
}

public class Cuadrado : IForma
{
    public double Lado { get; set; }
    
    public double CalcularArea() => Lado * Lado;
}
```

#### Interface Segregation Principle (ISP)
Los clientes no deben verse forzados a depender de interfaces que no utilizan.

```csharp
// ❌ Mal: Interface muy grande
public interface IWorker
{
    void Work();
    void Eat();
    void Sleep();
    void GetPaid();
    void TakeVacation();
}

// ✅ Bien: Interfaces pequeñas y específicas
public interface IWorkable
{
    void Work();
}

public interface IEatable
{
    void Eat();
}

public interface ISleepable
{
    void Sleep();
}

public interface IPayable
{
    void GetPaid();
}

public interface IVacationable
{
    void TakeVacation();
}

// Las clases implementan solo lo que necesitan
public class Human : IWorkable, IEatable, ISleepable, IPayable, IVacationable
{
    public void Work() { /* Implementación */ }
    public void Eat() { /* Implementación */ }
    public void Sleep() { /* Implementación */ }
    public void GetPaid() { /* Implementación */ }
    public void TakeVacation() { /* Implementación */ }
}

public class Robot : IWorkable, IPayable
{
    public void Work() { /* Implementación */ }
    public void GetPaid() { /* Implementación */ }
}
```

#### Dependency Inversion Principle (DIP)
Los módulos de alto nivel no deben depender de módulos de bajo nivel. Ambos deben depender de abstracciones.

```csharp
// ❌ Mal: Dependencia directa
public class EmailNotifier
{
    public void EnviarNotificacion(string mensaje)
    {
        // Lógica de envío de email
    }
}

public class UsuarioService
{
    private EmailNotifier _notifier = new EmailNotifier(); // Dependencia concreta
    
    public void CrearUsuario(Usuario usuario)
    {
        // Lógica de creación
        _notifier.EnviarNotificacion("Usuario creado");
    }
}

// ✅ Bien: Dependencia de abstracción
public interface INotifier
{
    void EnviarNotificacion(string mensaje);
}

public class EmailNotifier : INotifier
{
    public void EnviarNotificacion(string mensaje)
    {
        // Lógica de envío de email
    }
}

public class SMSNotifier : INotifier
{
    public void EnviarNotificacion(string mensaje)
    {
        // Lógica de envío de SMS
    }
}

public class UsuarioService
{
    private readonly INotifier _notifier;
    
    public UsuarioService(INotifier notifier) // Inyección de dependencia
    {
        _notifier = notifier;
    }
    
    public void CrearUsuario(Usuario usuario)
    {
        // Lógica de creación
        _notifier.EnviarNotificacion("Usuario creado");
    }
}
```

### 2. Patrones de Diseño Creacionales

#### Singleton Pattern
Asegura que una clase tenga solo una instancia y proporciona un punto de acceso global a ella.

```csharp
public class DatabaseConnection
{
    private static DatabaseConnection _instance;
    private static readonly object _lock = new object();
    
    private DatabaseConnection() { }
    
    public static DatabaseConnection Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DatabaseConnection();
                    }
                }
            }
            return _instance;
        }
    }
    
    public void Connect()
    {
        Console.WriteLine("Conectando a la base de datos...");
    }
}

// Uso
var connection = DatabaseConnection.Instance;
connection.Connect();
```

#### Factory Method Pattern
Define una interfaz para crear objetos, pero permite a las subclases decidir qué clase instanciar.

```csharp
public abstract class Document
{
    public abstract void Open();
    public abstract void Save();
}

public class PDFDocument : Document
{
    public override void Open() => Console.WriteLine("Abriendo PDF");
    public override void Save() => Console.WriteLine("Guardando PDF");
}

public class WordDocument : Document
{
    public override void Open() => Console.WriteLine("Abriendo Word");
    public override void Save() => Console.WriteLine("Guardando Word");
}

public abstract class DocumentCreator
{
    public abstract Document CreateDocument();
    
    public void ProcessDocument()
    {
        var document = CreateDocument();
        document.Open();
        document.Save();
    }
}

public class PDFCreator : DocumentCreator
{
    public override Document CreateDocument() => new PDFDocument();
}

public class WordCreator : DocumentCreator
{
    public override Document CreateDocument() => new WordDocument();
}
```

#### Abstract Factory Pattern
Proporciona una interfaz para crear familias de objetos relacionados sin especificar sus clases concretas.

```csharp
public interface IButton
{
    void Render();
}

public interface ITextBox
{
    void Render();
}

public class WindowsButton : IButton
{
    public void Render() => Console.WriteLine("Renderizando botón de Windows");
}

public class WindowsTextBox : ITextBox
{
    public void Render() => Console.WriteLine("Renderizando textbox de Windows");
}

public class MacButton : IButton
{
    public void Render() => Console.WriteLine("Renderizando botón de Mac");
}

public class MacTextBox : ITextBox
{
    public void Render() => Console.WriteLine("Renderizando textbox de Mac");
}

public interface IGUIFactory
{
    IButton CreateButton();
    ITextBox CreateTextBox();
}

public class WindowsFactory : IGUIFactory
{
    public IButton CreateButton() => new WindowsButton();
    public ITextBox CreateTextBox() => new WindowsTextBox();
}

public class MacFactory : IGUIFactory
{
    public IButton CreateButton() => new MacButton();
    public ITextBox CreateTextBox() => new MacTextBox();
}
```

### 3. Patrones de Diseño Estructurales

#### Adapter Pattern
Permite que interfaces incompatibles trabajen juntas.

```csharp
// Interfaz existente
public interface ILogger
{
    void Log(string message);
}

// Implementación existente
public class FileLogger : ILogger
{
    public void Log(string message)
    {
        File.AppendAllText("log.txt", message + Environment.NewLine);
    }
}

// Nueva interfaz que queremos usar
public interface INewLogger
{
    void LogInfo(string message);
    void LogError(string message);
    void LogWarning(string message);
}

// Adapter
public class LoggerAdapter : INewLogger
{
    private readonly ILogger _logger;
    
    public LoggerAdapter(ILogger logger)
    {
        _logger = logger;
    }
    
    public void LogInfo(string message) => _logger.Log($"INFO: {message}");
    public void LogError(string message) => _logger.Log($"ERROR: {message}");
    public void LogWarning(string message) => _logger.Log($"WARNING: {message}");
}
```

#### Decorator Pattern
Permite agregar comportamientos a objetos individuales dinámicamente.

```csharp
public interface ICoffee
{
    string GetDescription();
    double GetCost();
}

public class SimpleCoffee : ICoffee
{
    public string GetDescription() => "Café simple";
    public double GetCost() => 1.0;
}

public abstract class CoffeeDecorator : ICoffee
{
    protected ICoffee _coffee;
    
    public CoffeeDecorator(ICoffee coffee)
    {
        _coffee = coffee;
    }
    
    public virtual string GetDescription() => _coffee.GetDescription();
    public virtual double GetCost() => _coffee.GetCost();
}

public class MilkDecorator : CoffeeDecorator
{
    public MilkDecorator(ICoffee coffee) : base(coffee) { }
    
    public override string GetDescription() => base.GetDescription() + ", leche";
    public override double GetCost() => base.GetCost() + 0.5;
}

public class SugarDecorator : CoffeeDecorator
{
    public SugarDecorator(ICoffee coffee) : base(coffee) { }
    
    public override string GetDescription() => base.GetDescription() + ", azúcar";
    public override double GetCost() => base.GetCost() + 0.1;
}

// Uso
ICoffee coffee = new SimpleCoffee();
coffee = new MilkDecorator(coffee);
coffee = new SugarDecorator(coffee);

Console.WriteLine($"{coffee.GetDescription()}: ${coffee.GetCost()}");
```

### 4. Patrones de Diseño de Comportamiento

#### Observer Pattern
Define una dependencia uno-a-muchos entre objetos para que cuando un objeto cambie de estado, todos sus dependientes sean notificados.

```csharp
public interface IObserver
{
    void Update(string message);
}

public interface ISubject
{
    void Attach(IObserver observer);
    void Detach(IObserver observer);
    void Notify(string message);
}

public class NewsAgency : ISubject
{
    private List<IObserver> _observers = new List<IObserver>();
    
    public void Attach(IObserver observer)
    {
        _observers.Add(observer);
    }
    
    public void Detach(IObserver observer)
    {
        _observers.Remove(observer);
    }
    
    public void Notify(string message)
    {
        foreach (var observer in _observers)
        {
            observer.Update(message);
        }
    }
    
    public void PublishNews(string news)
    {
        Console.WriteLine($"Publicando noticia: {news}");
        Notify(news);
    }
}

public class NewsChannel : IObserver
{
    public string Name { get; set; }
    
    public NewsChannel(string name)
    {
        Name = name;
    }
    
    public void Update(string message)
    {
        Console.WriteLine($"{Name} recibió: {message}");
    }
}

// Uso
var agency = new NewsAgency();
var channel1 = new NewsChannel("CNN");
var channel2 = new NewsChannel("BBC");

agency.Attach(channel1);
agency.Attach(channel2);

agency.PublishNews("Nueva noticia importante!");
```

#### Strategy Pattern
Define una familia de algoritmos, encapsula cada uno y los hace intercambiables.

```csharp
public interface IPaymentStrategy
{
    void Pay(decimal amount);
}

public class CreditCardPayment : IPaymentStrategy
{
    public void Pay(decimal amount)
    {
        Console.WriteLine($"Pagando ${amount} con tarjeta de crédito");
    }
}

public class PayPalPayment : IPaymentStrategy
{
    public void Pay(decimal amount)
    {
        Console.WriteLine($"Pagando ${amount} con PayPal");
    }
}

public class CashPayment : IPaymentStrategy
{
    public void Pay(decimal amount)
    {
        Console.WriteLine($"Pagando ${amount} en efectivo");
    }
}

public class ShoppingCart
{
    private IPaymentStrategy _paymentStrategy;
    
    public void SetPaymentStrategy(IPaymentStrategy strategy)
    {
        _paymentStrategy = strategy;
    }
    
    public void Checkout(decimal amount)
    {
        _paymentStrategy?.Pay(amount);
    }
}

// Uso
var cart = new ShoppingCart();
cart.SetPaymentStrategy(new CreditCardPayment());
cart.Checkout(100.00m);

cart.SetPaymentStrategy(new PayPalPayment());
cart.Checkout(50.00m);
```

### 5. Patrones Arquitectónicos

#### Repository Pattern
Abstrae la lógica de acceso a datos, centralizando las operaciones comunes de acceso a datos.

```csharp
public interface IRepository<T> where T : class
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public class UserRepository : IRepository<User>
{
    private readonly DbContext _context;
    
    public UserRepository(DbContext context)
    {
        _context = context;
    }
    
    public User GetById(int id)
    {
        return _context.Users.Find(id);
    }
    
    public IEnumerable<User> GetAll()
    {
        return _context.Users.ToList();
    }
    
    public void Add(User entity)
    {
        _context.Users.Add(entity);
        _context.SaveChanges();
    }
    
    public void Update(User entity)
    {
        _context.Users.Update(entity);
        _context.SaveChanges();
    }
    
    public void Delete(User entity)
    {
        _context.Users.Remove(entity);
        _context.SaveChanges();
    }
}
```

#### Unit of Work Pattern
Mantiene una lista de objetos afectados por una transacción de negocio y coordina la escritura de cambios.

```csharp
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    int Complete();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private IUserRepository _users;
    private IProductRepository _products;
    
    public UnitOfWork(DbContext context)
    {
        _context = context;
    }
    
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IProductRepository Products => _products ??= new ProductRepository(_context);
    
    public int Complete()
    {
        return _context.SaveChanges();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Refactorización SOLID
Toma un código existente que viole los principios SOLID y refactorízalo para cumplirlos.

### Ejercicio 2: Implementación de Singleton Thread-Safe
Crea diferentes implementaciones de Singleton y compara su rendimiento y seguridad en entornos multi-hilo.

### Ejercicio 3: Factory Method para Diferentes Tipos de Conexiones
Implementa Factory Method para crear diferentes tipos de conexiones de base de datos.

### Ejercicio 4: Abstract Factory para UI
Crea un Abstract Factory para generar interfaces de usuario para diferentes sistemas operativos.

### Ejercicio 5: Adapter para APIs Externas
Implementa el patrón Adapter para integrar diferentes APIs de servicios externos.

### Ejercicio 6: Decorator para Sistema de Logging
Crea un sistema de logging usando el patrón Decorator con diferentes niveles y formatos.

### Ejercicio 7: Observer para Sistema de Notificaciones
Implementa un sistema de notificaciones usando Observer para diferentes tipos de eventos.

### Ejercicio 8: Strategy para Algoritmos de Ordenamiento
Crea diferentes estrategias de ordenamiento y permite al usuario elegir cuál usar.

### Ejercicio 9: Repository y Unit of Work
Implementa un sistema completo de persistencia usando Repository y Unit of Work.

### Ejercicio 10: Proyecto Integrador - Sistema de E-commerce
Crea un sistema completo que incluya:
- Aplicación de todos los principios SOLID
- Implementación de múltiples patrones de diseño
- Arquitectura limpia y mantenible
- Sistema de pagos con Strategy
- Notificaciones con Observer
- Persistencia con Repository y Unit of Work

## 📝 Quiz de Autoevaluación

1. ¿Cuál es la diferencia entre Factory Method y Abstract Factory?
2. ¿Por qué es importante el principio de inversión de dependencias?
3. ¿Cuándo usarías el patrón Decorator en lugar de herencia?
4. ¿Qué ventajas tiene el patrón Repository?
5. ¿Cómo aseguras que un Singleton sea thread-safe?

## 🚀 Siguiente Nivel

Una vez que hayas completado todos los ejercicios y comprendas los conceptos, estarás listo para el **Senior Level 2: Testing y TDD**.

## 💡 Consejos de Estudio

- Practica refactorizando código existente para aplicar SOLID
- Implementa patrones de diseño en proyectos reales
- Estudia ejemplos de código de frameworks populares
- Crea diagramas UML para visualizar los patrones
- Experimenta con diferentes combinaciones de patrones

¡Estás desarrollando habilidades de arquitecto de software! 🚀
