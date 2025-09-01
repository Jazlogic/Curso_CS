# 🚀 Clase 1: Patrones de Diseño Intermedios

## 📋 Información de la Clase

- **Módulo**: Mid Level 1 - Programación Avanzada y Patrones de Diseño
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Módulo 3 (Junior Level 3)

## 🎯 Objetivos de Aprendizaje

- Dominar patrones de diseño intermedios y avanzados
- Implementar el patrón Strategy para algoritmos intercambiables
- Aplicar el patrón Command para encapsular solicitudes
- Utilizar el patrón Chain of Responsibility para manejo de solicitudes
- Crear sistemas flexibles y extensibles con patrones de diseño

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| **Clase 1** | **Patrones de Diseño Intermedios** | ← Estás aquí |
| [Clase 2](clase_2_programacion_asincrona_avanzada.md) | Programación Asíncrona Avanzada | Siguiente → |
| [Clase 3](clase_3_programacion_paralela.md) | Programación Paralela y TPL | |
| [Clase 4](clase_4_clean_architecture.md) | Clean Architecture | |
| [Clase 5](clase_5_dependency_injection.md) | Dependency Injection Avanzada | |
| [Clase 6](clase_6_logging_monitoreo.md) | Logging y Monitoreo | |
| [Clase 7](clase_7_refactoring_clean_code.md) | Refactoring y Clean Code | |
| [Clase 8](clase_8_testing_integracion.md) | Testing de Integración | |
| [Clase 9](clase_9_testing_comportamiento.md) | Testing de Comportamiento (BDD) | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de E-commerce | |

**← [Volver al README del Módulo 4](../midLevel_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Patrón Strategy

El patrón Strategy permite definir una familia de algoritmos, encapsular cada uno y hacerlos intercambiables.

```csharp
// Interfaz para estrategias de pago
public interface IPaymentStrategy
{
    decimal ProcessPayment(decimal amount);
    string GetPaymentMethod();
}

// Estrategia de pago con tarjeta de crédito
public class CreditCardPaymentStrategy : IPaymentStrategy
{
    private readonly string _cardNumber;
    private readonly string _expiryDate;
    private readonly string _cvv;
    
    public CreditCardPaymentStrategy(string cardNumber, string expiryDate, string cvv)
    {
        _cardNumber = cardNumber;
        _expiryDate = expiryDate;
        _cvv = cvv;
    }
    
    public decimal ProcessPayment(decimal amount)
    {
        // Simular procesamiento de tarjeta de crédito
        Console.WriteLine($"Procesando pago de ${amount} con tarjeta terminada en {_cardNumber.Substring(_cardNumber.Length - 4)}");
        
        // Simular comisión del 2.5%
        var comision = amount * 0.025m;
        var total = amount + comision;
        
        Console.WriteLine($"Comisión: ${comision:F2}");
        Console.WriteLine($"Total: ${total:F2}");
        
        return total;
    }
    
    public string GetPaymentMethod()
    {
        return "Tarjeta de Crédito";
    }
}

// Estrategia de pago con PayPal
public class PayPalPaymentStrategy : IPaymentStrategy
{
    private readonly string _email;
    
    public PayPalPaymentStrategy(string email)
    {
        _email = email;
    }
    
    public decimal ProcessPayment(decimal amount)
    {
        // Simular procesamiento de PayPal
        Console.WriteLine($"Procesando pago de ${amount} con PayPal: {_email}");
        
        // Simular comisión del 3.5%
        var comision = amount * 0.035m;
        var total = amount + comision;
        
        Console.WriteLine($"Comisión: ${comision:F2}");
        Console.WriteLine($"Total: ${total:F2}");
        
        return total;
    }
    
    public string GetPaymentMethod()
    {
        return "PayPal";
    }
}

// Estrategia de pago con transferencia bancaria
public class BankTransferPaymentStrategy : IPaymentStrategy
{
    private readonly string _accountNumber;
    private readonly string _bankName;
    
    public BankTransferPaymentStrategy(string accountNumber, string bankName)
    {
        _accountNumber = accountNumber;
        _bankName = bankName;
    }
    
    public decimal ProcessPayment(decimal amount)
    {
        // Simular procesamiento de transferencia bancaria
        Console.WriteLine($"Procesando transferencia de ${amount} a {_bankName}");
        Console.WriteLine($"Número de cuenta: {_accountNumber}");
        
        // Sin comisión para transferencias bancarias
        Console.WriteLine($"Comisión: $0.00");
        Console.WriteLine($"Total: ${amount:F2}");
        
        return amount;
    }
    
    public string GetPaymentMethod()
    {
        return "Transferencia Bancaria";
    }
}

// Contexto que usa las estrategias
public class PaymentProcessor
{
    private IPaymentStrategy _paymentStrategy;
    
    public void SetPaymentStrategy(IPaymentStrategy strategy)
    {
        _paymentStrategy = strategy;
    }
    
    public decimal ProcessPayment(decimal amount)
    {
        if (_paymentStrategy == null)
        {
            throw new InvalidOperationException("Estrategia de pago no configurada");
        }
        
        Console.WriteLine($"=== Procesando Pago ===");
        Console.WriteLine($"Método: {_paymentStrategy.GetPaymentMethod()}");
        Console.WriteLine($"Monto: ${amount:F2}");
        
        var result = _paymentStrategy.ProcessPayment(amount);
        
        Console.WriteLine($"=== Pago Completado ===");
        return result;
    }
}

// Uso del patrón Strategy
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Patrón Strategy - Métodos de Pago ===\n");
        
        var paymentProcessor = new PaymentProcessor();
        var amount = 100.00m;
        
        // Procesar pago con tarjeta de crédito
        Console.WriteLine("1. Pago con Tarjeta de Crédito:");
        var creditCardStrategy = new CreditCardPaymentStrategy("1234567890123456", "12/25", "123");
        paymentProcessor.SetPaymentStrategy(creditCardStrategy);
        var creditCardTotal = paymentProcessor.ProcessPayment(amount);
        
        Console.WriteLine();
        
        // Procesar pago con PayPal
        Console.WriteLine("2. Pago con PayPal:");
        var payPalStrategy = new PayPalPaymentStrategy("usuario@email.com");
        paymentProcessor.SetPaymentStrategy(payPalStrategy);
        var payPalTotal = paymentProcessor.ProcessPayment(amount);
        
        Console.WriteLine();
        
        // Procesar pago con transferencia bancaria
        Console.WriteLine("3. Pago con Transferencia Bancaria:");
        var bankTransferStrategy = new BankTransferPaymentStrategy("987654321", "Banco Nacional");
        paymentProcessor.SetPaymentStrategy(bankTransferStrategy);
        var bankTransferTotal = paymentProcessor.ProcessPayment(amount);
        
        Console.WriteLine();
        
        // Comparar costos
        Console.WriteLine("=== Comparación de Costos ===");
        Console.WriteLine($"Tarjeta de Crédito: ${creditCardTotal:F2}");
        Console.WriteLine($"PayPal: ${payPalTotal:F2}");
        Console.WriteLine($"Transferencia Bancaria: ${bankTransferTotal:F2}");
        
        var masEconomico = new[] { creditCardTotal, payPalTotal, bankTransferTotal }.Min();
        Console.WriteLine($"\nOpción más económica: ${masEconomico:F2}");
    }
}
```

### 2. Patrón Command

El patrón Command encapsula una solicitud como un objeto, permitiendo parametrizar clientes con diferentes solicitudes.

```csharp
// Interfaz para comandos
public interface ICommand
{
    void Execute();
    void Undo();
    bool CanExecute();
}

// Comando para encender dispositivo
public class TurnOnCommand : ICommand
{
    private readonly IDevice _device;
    
    public TurnOnCommand(IDevice device)
    {
        _device = device;
    }
    
    public void Execute()
    {
        if (CanExecute())
        {
            _device.TurnOn();
            Console.WriteLine($"Dispositivo {_device.Name} encendido");
        }
    }
    
    public void Undo()
    {
        if (_device.IsOn)
        {
            _device.TurnOff();
            Console.WriteLine($"Dispositivo {_device.Name} apagado (deshacer)");
        }
    }
    
    public bool CanExecute()
    {
        return !_device.IsOn;
    }
}

// Comando para apagar dispositivo
public class TurnOffCommand : ICommand
{
    private readonly IDevice _device;
    
    public TurnOffCommand(IDevice device)
    {
        _device = device;
    }
    
    public void Execute()
    {
        if (CanExecute())
        {
            _device.TurnOff();
            Console.WriteLine($"Dispositivo {_device.Name} apagado");
        }
    }
    
    public void Undo()
    {
        if (!_device.IsOn)
        {
            _device.TurnOn();
            Console.WriteLine($"Dispositivo {_device.Name} encendido (deshacer)");
        }
    }
    
    public bool CanExecute()
    {
        return _device.IsOn;
    }
}

// Comando para ajustar volumen
public class AdjustVolumeCommand : ICommand
{
    private readonly IDevice _device;
    private readonly int _volumeChange;
    private int _previousVolume;
    
    public AdjustVolumeCommand(IDevice device, int volumeChange)
    {
        _device = device;
        _volumeChange = volumeChange;
    }
    
    public void Execute()
    {
        if (CanExecute())
        {
            _previousVolume = _device.Volume;
            var newVolume = Math.Max(0, Math.Min(100, _device.Volume + _volumeChange));
            _device.SetVolume(newVolume);
            
            Console.WriteLine($"Volumen de {_device.Name} ajustado de {_previousVolume} a {_device.Volume}");
        }
    }
    
    public void Undo()
    {
        _device.SetVolume(_previousVolume);
        Console.WriteLine($"Volumen de {_device.Name} restaurado a {_previousVolume}");
    }
    
    public bool CanExecute()
    {
        return _device.IsOn;
    }
}

// Comando compuesto (macro)
public class MacroCommand : ICommand
{
    private readonly List<ICommand> _commands;
    
    public MacroCommand(params ICommand[] commands)
    {
        _commands = new List<ICommand>(commands);
    }
    
    public void Execute()
    {
        Console.WriteLine("=== Ejecutando Macro ===");
        foreach (var command in _commands)
        {
            if (command.CanExecute())
            {
                command.Execute();
            }
        }
        Console.WriteLine("=== Macro Completada ===");
    }
    
    public void Undo()
    {
        Console.WriteLine("=== Deshaciendo Macro ===");
        // Deshacer en orden inverso
        for (int i = _commands.Count - 1; i >= 0; i--)
        {
            _commands[i].Undo();
        }
        Console.WriteLine("=== Macro Deshecha ===");
    }
    
    public bool CanExecute()
    {
        return _commands.All(c => c.CanExecute());
    }
}

// Interfaces para dispositivos
public interface IDevice
{
    string Name { get; }
    bool IsOn { get; }
    int Volume { get; }
    void TurnOn();
    void TurnOff();
    void SetVolume(int volume);
}

// Implementación de dispositivo
public class SmartTV : IDevice
{
    public string Name { get; }
    public bool IsOn { get; private set; }
    public int Volume { get; private set; }
    
    public SmartTV(string name)
    {
        Name = name;
        IsOn = false;
        Volume = 50;
    }
    
    public void TurnOn()
    {
        IsOn = true;
    }
    
    public void TurnOff()
    {
        IsOn = false;
    }
    
    public void SetVolume(int volume)
    {
        Volume = Math.Max(0, Math.Min(100, volume));
    }
}

// Invocador que ejecuta comandos
public class RemoteControl
{
    private readonly Dictionary<string, ICommand> _commands;
    private readonly Stack<ICommand> _commandHistory;
    
    public RemoteControl()
    {
        _commands = new Dictionary<string, ICommand>();
        _commandHistory = new Stack<ICommand>();
    }
    
    public void SetCommand(string button, ICommand command)
    {
        _commands[button] = command;
    }
    
    public void PressButton(string button)
    {
        if (_commands.TryGetValue(button, out var command))
        {
            if (command.CanExecute())
            {
                command.Execute();
                _commandHistory.Push(command);
            }
            else
            {
                Console.WriteLine($"Comando no puede ejecutarse para el botón {button}");
            }
        }
        else
        {
            Console.WriteLine($"Botón {button} no configurado");
        }
    }
    
    public void UndoLastCommand()
    {
        if (_commandHistory.Count > 0)
        {
            var lastCommand = _commandHistory.Pop();
            lastCommand.Undo();
        }
        else
        {
            Console.WriteLine("No hay comandos para deshacer");
        }
    }
}

// Uso del patrón Command
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Patrón Command - Control Remoto Inteligente ===\n");
        
        // Crear dispositivos
        var tv = new SmartTV("Samsung Smart TV");
        
        // Crear comandos
        var turnOnCommand = new TurnOnCommand(tv);
        var turnOffCommand = new TurnOffCommand(tv);
        var volumeUpCommand = new AdjustVolumeCommand(tv, 10);
        var volumeDownCommand = new AdjustVolumeCommand(tv, -10);
        
        // Crear macro para configuración de película
        var movieModeMacro = new MacroCommand(
            turnOnCommand,
            volumeUpCommand,
            volumeUpCommand
        );
        
        // Configurar control remoto
        var remote = new RemoteControl();
        remote.SetCommand("Power", turnOnCommand);
        remote.SetCommand("PowerOff", turnOffCommand);
        remote.SetCommand("Vol+", volumeUpCommand);
        remote.SetCommand("Vol-", volumeDownCommand);
        remote.SetCommand("Movie", movieModeMacro);
        
        // Usar el control remoto
        Console.WriteLine("1. Encender TV:");
        remote.PressButton("Power");
        
        Console.WriteLine("\n2. Subir volumen:");
        remote.PressButton("Vol+");
        
        Console.WriteLine("\n3. Ejecutar modo película:");
        remote.PressButton("Movie");
        
        Console.WriteLine("\n4. Deshacer último comando:");
        remote.UndoLastCommand();
        
        Console.WriteLine("\n5. Apagar TV:");
        remote.PressButton("PowerOff");
    }
}
```

### 3. Patrón Chain of Responsibility

El patrón Chain of Responsibility permite pasar solicitudes a lo largo de una cadena de manejadores.

```csharp
// Interfaz para manejadores en la cadena
public abstract class SupportHandler
{
    protected SupportHandler _nextHandler;
    
    public void SetNext(SupportHandler handler)
    {
        _nextHandler = handler;
    }
    
    public abstract void HandleRequest(SupportTicket ticket);
    
    protected void PassToNext(SupportTicket ticket)
    {
        if (_nextHandler != null)
        {
            _nextHandler.HandleRequest(ticket);
        }
        else
        {
            Console.WriteLine($"Ticket #{ticket.Id} no pudo ser procesado por ningún nivel de soporte");
        }
    }
}

// Clase para tickets de soporte
public class SupportTicket
{
    public int Id { get; set; }
    public string Description { get; set; }
    public TicketPriority Priority { get; set; }
    public string CustomerEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public SupportTicket(int id, string description, TicketPriority priority, string customerEmail)
    {
        Id = id;
        Description = description;
        Priority = priority;
        CustomerEmail = customerEmail;
        CreatedAt = DateTime.Now;
    }
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

// Manejador de nivel 1 (Soporte Básico)
public class Level1SupportHandler : SupportHandler
{
    public override void HandleRequest(SupportTicket ticket)
    {
        if (ticket.Priority == TicketPriority.Low)
        {
            Console.WriteLine($"Ticket #{ticket.Id} manejado por Soporte Nivel 1");
            Console.WriteLine($"Descripción: {ticket.Description}");
            Console.WriteLine($"Solución: Consultar base de conocimientos");
            Console.WriteLine($"Email enviado a: {ticket.CustomerEmail}");
        }
        else
        {
            Console.WriteLine($"Ticket #{ticket.Id} transferido al siguiente nivel por Soporte Nivel 1");
            PassToNext(ticket);
        }
    }
}

// Manejador de nivel 2 (Soporte Técnico)
public class Level2SupportHandler : SupportHandler
{
    public override void HandleRequest(SupportTicket ticket)
    {
        if (ticket.Priority == TicketPriority.Medium)
        {
            Console.WriteLine($"Ticket #{ticket.Id} manejado por Soporte Nivel 2");
            Console.WriteLine($"Descripción: {ticket.Description}");
            Console.WriteLine($"Solución: Análisis técnico detallado");
            Console.WriteLine($"Email enviado a: {ticket.CustomerEmail}");
        }
        else
        {
            Console.WriteLine($"Ticket #{ticket.Id} transferido al siguiente nivel por Soporte Nivel 2");
            PassToNext(ticket);
        }
    }
}

// Manejador de nivel 3 (Soporte Especializado)
public class Level3SupportHandler : SupportHandler
{
    public override void HandleRequest(SupportTicket ticket)
    {
        if (ticket.Priority == TicketPriority.High)
        {
            Console.WriteLine($"Ticket #{ticket.Id} manejado por Soporte Nivel 3");
            Console.WriteLine($"Descripción: {ticket.Description}");
            Console.WriteLine($"Solución: Intervención de especialista");
            Console.WriteLine($"Email enviado a: {ticket.CustomerEmail}");
        }
        else
        {
            Console.WriteLine($"Ticket #{ticket.Id} transferido al siguiente nivel por Soporte Nivel 3");
            PassToNext(ticket);
        }
    }
}

// Manejador de nivel 4 (Soporte de Emergencia)
public class EmergencySupportHandler : SupportHandler
{
    public override void HandleRequest(SupportTicket ticket)
    {
        if (ticket.Priority == TicketPriority.Critical)
        {
            Console.WriteLine($"🚨 Ticket #{ticket.Id} manejado por Soporte de Emergencia 🚨");
            Console.WriteLine($"Descripción: {ticket.Description}");
            Console.WriteLine($"Solución: Intervención inmediata del equipo senior");
            Console.WriteLine($"Email enviado a: {ticket.CustomerEmail}");
            Console.WriteLine($"Llamada telefónica programada para: {DateTime.Now.AddMinutes(30)}");
        }
        else
        {
            Console.WriteLine($"Ticket #{ticket.Id} transferido al siguiente nivel por Soporte de Emergencia");
            PassToNext(ticket);
        }
    }
}

// Sistema de soporte que configura la cadena
public class SupportSystem
{
    private SupportHandler _firstHandler;
    
    public void SetupChain()
    {
        var level1 = new Level1SupportHandler();
        var level2 = new Level2SupportHandler();
        var level3 = new Level3SupportHandler();
        var emergency = new EmergencySupportHandler();
        
        // Configurar la cadena
        level1.SetNext(level2);
        level2.SetNext(level3);
        level3.SetNext(emergency);
        
        _firstHandler = level1;
    }
    
    public void ProcessTicket(SupportTicket ticket)
    {
        Console.WriteLine($"\n=== Procesando Ticket #{ticket.Id} ===");
        Console.WriteLine($"Prioridad: {ticket.Priority}");
        Console.WriteLine($"Descripción: {ticket.Description}");
        Console.WriteLine($"Cliente: {ticket.CustomerEmail}");
        Console.WriteLine($"Fecha: {ticket.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine(new string('-', 50));
        
        _firstHandler.HandleRequest(ticket);
        
        Console.WriteLine(new string('-', 50));
    }
}

// Uso del patrón Chain of Responsibility
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Patrón Chain of Responsibility - Sistema de Soporte ===\n");
        
        var supportSystem = new SupportSystem();
        supportSystem.SetupChain();
        
        // Crear tickets de diferentes prioridades
        var tickets = new List<SupportTicket>
        {
            new SupportTicket(1, "No puedo acceder a mi cuenta", TicketPriority.Low, "cliente1@email.com"),
            new SupportTicket(2, "La aplicación se cierra inesperadamente", TicketPriority.Medium, "cliente2@email.com"),
            new SupportTicket(3, "Error crítico en el sistema de pagos", TicketPriority.High, "cliente3@email.com"),
            new SupportTicket(4, "Sistema completamente caído", TicketPriority.Critical, "cliente4@email.com"),
            new SupportTicket(5, "Problema con la configuración", TicketPriority.Low, "cliente5@email.com")
        };
        
        // Procesar todos los tickets
        foreach (var ticket in tickets)
        {
            supportSystem.ProcessTicket(ticket);
        }
        
        Console.WriteLine("\n=== Resumen de Tickets Procesados ===");
        Console.WriteLine($"Total de tickets: {tickets.Count}");
        Console.WriteLine($"Tickets críticos: {tickets.Count(t => t.Priority == TicketPriority.Critical)}");
        Console.WriteLine($"Tickets de alta prioridad: {tickets.Count(t => t.Priority == TicketPriority.High)}");
        Console.WriteLine($"Tickets de prioridad media: {tickets.Count(t => t.Priority == TicketPriority.Medium)}");
        Console.WriteLine($"Tickets de baja prioridad: {tickets.Count(t => t.Priority == TicketPriority.Low)}");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Sistema de Filtros con Strategy
Implementa un sistema de filtros de imágenes usando el patrón Strategy que permita aplicar diferentes algoritmos (blanco y negro, sepia, alto contraste).

### Ejercicio 2: Editor de Texto con Command
Crea un editor de texto simple que implemente el patrón Command para operaciones como escribir, borrar, copiar y pegar, con capacidad de deshacer/rehacer.

### Ejercicio 3: Sistema de Validación con Chain of Responsibility
Desarrolla un sistema de validación de formularios que use Chain of Responsibility para validar campos en secuencia (requerido, formato, longitud, etc.).

## 🔍 Puntos Clave

1. **Strategy Pattern** permite intercambiar algoritmos en tiempo de ejecución
2. **Command Pattern** encapsula solicitudes como objetos para operaciones reversibles
3. **Chain of Responsibility** distribuye responsabilidades en una cadena de manejadores
4. **Los patrones** hacen el código más flexible, mantenible y extensible
5. **La combinación** de patrones permite crear arquitecturas robustas y escalables

## 📚 Recursos Adicionales

- [Design Patterns - Gang of Four](https://en.wikipedia.org/wiki/Design_Patterns)
- [Strategy Pattern - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/standard/modern-web-apps-azure/architectural-principles#command-query-separation)
- [Command Pattern - Refactoring Guru](https://refactoring.guru/design-patterns/command)

---

**🎯 ¡Has completado la Clase 1! Ahora dominas los patrones de diseño intermedios**

**📚 [Siguiente: Clase 2 - Programación Asíncrona Avanzada](clase_2_programacion_asincrona_avanzada.md)**
