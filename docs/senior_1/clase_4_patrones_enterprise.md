# 🚀 Clase 4: Patrones de Diseño Enterprise

## 📋 Información de la Clase

- **Módulo**: Senior 1 - Arquitectura de Software Empresarial
- **Duración**: 2 horas
- **Nivel**: Senior
- **Prerrequisitos**: Completar Clase 3 (Arquitectura de Microservicios Avanzada)

## 🎯 Objetivos de Aprendizaje

- Implementar patrones arquitectónicos empresariales
- Identificar y evitar anti-patrones
- Aplicar patrones de integración avanzados
- Implementar patrones de concurrencia empresariales

---

## 📚 Navegación del Módulo 6

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_limpia_avanzada.md) | Arquitectura Limpia Avanzada | |
| [Clase 2](clase_2_event_driven_architecture.md) | Event-Driven Architecture | |
| [Clase 3](clase_3_microservicios_avanzada.md) | Arquitectura de Microservicios Avanzada | ← Anterior |
| **Clase 4** | **Patrones de Diseño Enterprise** | ← Estás aquí |
| [Clase 5](clase_5_arquitectura_datos_avanzada.md) | Arquitectura de Datos Avanzada | Siguiente → |
| [Clase 6](clase_6_calidad_codigo_metricas.md) | Calidad del Código y Métricas | |
| [Clase 7](clase_7_monitoreo_observabilidad.md) | Monitoreo y Observabilidad | |
| [Clase 8](clase_8_arquitectura_evolutiva.md) | Arquitectura Evolutiva | |
| [Clase 9](clase_9_seguridad_enterprise.md) | Arquitectura de Seguridad Enterprise | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Plataforma Empresarial | |

**← [Volver al README del Módulo 6](../senior_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Patrones de Diseño Enterprise

Los patrones empresariales proporcionan soluciones probadas para problemas comunes en aplicaciones de nivel enterprise.

```csharp
// ===== PATRONES DE DISEÑO ENTERPRISE - IMPLEMENTACIÓN COMPLETA =====
namespace EnterprisePatterns
{
    // ===== PATRÓN COMPOSITE =====
    namespace CompositePattern
    {
        public abstract class Component
        {
            public string Name { get; protected set; }
            public abstract void Operation();
            public abstract void Add(Component component);
            public abstract void Remove(Component component);
            public abstract Component GetChild(int index);
        }
        
        public class Leaf : Component
        {
            public Leaf(string name)
            {
                Name = name;
            }
            
            public override void Operation()
            {
                Console.WriteLine($"Leaf {Name} performing operation");
            }
            
            public override void Add(Component component)
            {
                throw new InvalidOperationException("Cannot add to leaf");
            }
            
            public override void Remove(Component component)
            {
                throw new InvalidOperationException("Cannot remove from leaf");
            }
            
            public override Component GetChild(int index)
            {
                throw new InvalidOperationException("Leaf has no children");
            }
        }
        
        public class Composite : Component
        {
            private readonly List<Component> _children = new();
            
            public Composite(string name)
            {
                Name = name;
            }
            
            public override void Operation()
            {
                Console.WriteLine($"Composite {Name} performing operation");
                foreach (var child in _children)
                {
                    child.Operation();
                }
            }
            
            public override void Add(Component component)
            {
                _children.Add(component);
            }
            
            public override void Remove(Component component)
            {
                _children.Remove(component);
            }
            
            public override Component GetChild(int index)
            {
                return _children[index];
            }
            
            public int ChildrenCount => _children.Count;
        }
    }
    
    // ===== PATRÓN DECORATOR =====
    namespace DecoratorPattern
    {
        public interface IComponent
        {
            void Operation();
        }
        
        public class ConcreteComponent : IComponent
        {
            public void Operation()
            {
                Console.WriteLine("ConcreteComponent operation");
            }
        }
        
        public abstract class Decorator : IComponent
        {
            protected readonly IComponent _component;
            
            protected Decorator(IComponent component)
            {
                _component = component;
            }
            
            public virtual void Operation()
            {
                _component.Operation();
            }
        }
        
        public class ConcreteDecoratorA : Decorator
        {
            public ConcreteDecoratorA(IComponent component) : base(component) { }
            
            public override void Operation()
            {
                Console.WriteLine("ConcreteDecoratorA before operation");
                base.Operation();
                Console.WriteLine("ConcreteDecoratorA after operation");
            }
        }
        
        public class ConcreteDecoratorB : Decorator
        {
            public ConcreteDecoratorB(IComponent component) : base(component) { }
            
            public override void Operation()
            {
                Console.WriteLine("ConcreteDecoratorB before operation");
                base.Operation();
                Console.WriteLine("ConcreteDecoratorB after operation");
            }
        }
    }
    
    // ===== PATRÓN FACADE =====
    namespace FacadePattern
    {
        public class SubsystemA
        {
            public void OperationA1()
            {
                Console.WriteLine("SubsystemA.OperationA1");
            }
            
            public void OperationA2()
            {
                Console.WriteLine("SubsystemA.OperationA2");
            }
        }
        
        public class SubsystemB
        {
            public void OperationB1()
            {
                Console.WriteLine("SubsystemB.OperationB1");
            }
            
            public void OperationB2()
            {
                Console.WriteLine("SubsystemB.OperationB2");
            }
        }
        
        public class SubsystemC
        {
            public void OperationC1()
            {
                Console.WriteLine("SubsystemC.OperationC1");
            }
            
            public void OperationC2()
            {
                Console.WriteLine("SubsystemC.OperationC2");
            }
        }
        
        public class Facade
        {
            private readonly SubsystemA _subsystemA;
            private readonly SubsystemB _subsystemB;
            private readonly SubsystemC _subsystemC;
            
            public Facade()
            {
                _subsystemA = new SubsystemA();
                _subsystemB = new SubsystemB();
                _subsystemC = new SubsystemC();
            }
            
            public void Operation1()
            {
                Console.WriteLine("Facade.Operation1");
                _subsystemA.OperationA1();
                _subsystemB.OperationB1();
            }
            
            public void Operation2()
            {
                Console.WriteLine("Facade.Operation2");
                _subsystemB.OperationB2();
                _subsystemC.OperationC1();
            }
        }
    }
    
    // ===== PATRÓN FLYWEIGHT =====
    namespace FlyweightPattern
    {
        public interface IFlyweight
        {
            void Operation(string extrinsicState);
        }
        
        public class ConcreteFlyweight : IFlyweight
        {
            private readonly string _intrinsicState;
            
            public ConcreteFlyweight(string intrinsicState)
            {
                _intrinsicState = intrinsicState;
            }
            
            public void Operation(string extrinsicState)
            {
                Console.WriteLine($"ConcreteFlyweight: intrinsic={_intrinsicState}, extrinsic={extrinsicState}");
            }
        }
        
        public class FlyweightFactory
        {
            private readonly Dictionary<string, IFlyweight> _flyweights = new();
            
            public IFlyweight GetFlyweight(string key)
            {
                if (!_flyweights.ContainsKey(key))
                {
                    _flyweights[key] = new ConcreteFlyweight(key);
                }
                
                return _flyweights[key];
            }
            
            public int FlyweightCount => _flyweights.Count;
        }
    }
    
    // ===== PATRÓN PROXY =====
    namespace ProxyPattern
    {
        public interface ISubject
        {
            void Request();
        }
        
        public class RealSubject : ISubject
        {
            public void Request()
            {
                Console.WriteLine("RealSubject: Handling request");
            }
        }
        
        public class Proxy : ISubject
        {
            private RealSubject _realSubject;
            private readonly ILogger<Proxy> _logger;
            
            public Proxy(ILogger<Proxy> logger)
            {
                _logger = logger;
            }
            
            public void Request()
            {
                if (_realSubject == null)
                {
                    _realSubject = new RealSubject();
                }
                
                _logger.LogInformation("Proxy: Logging before request");
                _realSubject.Request();
                _logger.LogInformation("Proxy: Logging after request");
            }
        }
        
        public class VirtualProxy : ISubject
        {
            private RealSubject _realSubject;
            
            public void Request()
            {
                if (_realSubject == null)
                {
                    Console.WriteLine("VirtualProxy: Creating RealSubject");
                    _realSubject = new RealSubject();
                }
                
                _realSubject.Request();
            }
        }
        
        public class ProtectionProxy : ISubject
        {
            private readonly RealSubject _realSubject;
            private readonly string _accessLevel;
            
            public ProtectionProxy(string accessLevel)
            {
                _realSubject = new RealSubject();
                _accessLevel = accessLevel;
            }
            
            public void Request()
            {
                if (_accessLevel == "admin")
                {
                    _realSubject.Request();
                }
                else
                {
                    Console.WriteLine("ProtectionProxy: Access denied");
                }
            }
        }
    }
    
    // ===== PATRÓN CHAIN OF RESPONSIBILITY =====
    namespace ChainOfResponsibilityPattern
    {
        public abstract class Handler
        {
            protected Handler _successor;
            
            public void SetSuccessor(Handler successor)
            {
                _successor = successor;
            }
            
            public abstract void HandleRequest(int request);
        }
        
        public class ConcreteHandlerA : Handler
        {
            public override void HandleRequest(int request)
            {
                if (request >= 0 && request < 10)
                {
                    Console.WriteLine($"ConcreteHandlerA handled request {request}");
                }
                else if (_successor != null)
                {
                    _successor.HandleRequest(request);
                }
            }
        }
        
        public class ConcreteHandlerB : Handler
        {
            public override void HandleRequest(int request)
            {
                if (request >= 10 && request < 20)
                {
                    Console.WriteLine($"ConcreteHandlerB handled request {request}");
                }
                else if (_successor != null)
                {
                    _successor.HandleRequest(request);
                }
            }
        }
        
        public class ConcreteHandlerC : Handler
        {
            public override void HandleRequest(int request)
            {
                if (request >= 20 && request < 30)
                {
                    Console.WriteLine($"ConcreteHandlerC handled request {request}");
                }
                else if (_successor != null)
                {
                    _successor.HandleRequest(request);
                }
                else
                {
                    Console.WriteLine($"No handler found for request {request}");
                }
            }
        }
    }
    
    // ===== PATRÓN COMMAND =====
    namespace CommandPattern
    {
        public interface ICommand
        {
            void Execute();
            void Undo();
        }
        
        public class Receiver
        {
            public void Action()
            {
                Console.WriteLine("Receiver: Action executed");
            }
            
            public void ReverseAction()
            {
                Console.WriteLine("Receiver: Action reversed");
            }
        }
        
        public class ConcreteCommand : ICommand
        {
            private readonly Receiver _receiver;
            
            public ConcreteCommand(Receiver receiver)
            {
                _receiver = receiver;
            }
            
            public void Execute()
            {
                _receiver.Action();
            }
            
            public void Undo()
            {
                _receiver.ReverseAction();
            }
        }
        
        public class Invoker
        {
            private readonly List<ICommand> _commands = new();
            
            public void StoreAndExecute(ICommand command)
            {
                _commands.Add(command);
                command.Execute();
            }
            
            public void UndoLast()
            {
                if (_commands.Count > 0)
                {
                    var lastCommand = _commands[^1];
                    lastCommand.Undo();
                    _commands.RemoveAt(_commands.Count - 1);
                }
            }
        }
    }
    
    // ===== PATRÓN INTERPRETER =====
    namespace InterpreterPattern
    {
        public abstract class Expression
        {
            public abstract int Interpret();
        }
        
        public class NumberExpression : Expression
        {
            private readonly int _number;
            
            public NumberExpression(int number)
            {
                _number = number;
            }
            
            public override int Interpret()
            {
                return _number;
            }
        }
        
        public class AddExpression : Expression
        {
            private readonly Expression _left;
            private readonly Expression _right;
            
            public AddExpression(Expression left, Expression right)
            {
                _left = left;
                _right = right;
            }
            
            public override int Interpret()
            {
                return _left.Interpret() + _right.Interpret();
            }
        }
        
        public class SubtractExpression : Expression
        {
            private readonly Expression _left;
            private readonly Expression _right;
            
            public SubtractExpression(Expression left, Expression right)
            {
                _left = left;
                _right = right;
            }
            
            public override int Interpret()
            {
                return _left.Interpret() - _right.Interpret();
            }
        }
    }
    
    // ===== PATRÓN ITERATOR =====
    namespace IteratorPattern
    {
        public interface IIterator<T>
        {
            T First();
            T Next();
            bool IsDone();
            T CurrentItem();
        }
        
        public interface IAggregate<T>
        {
            IIterator<T> CreateIterator();
        }
        
        public class ConcreteAggregate<T> : IAggregate<T>
        {
            private readonly List<T> _items = new();
            
            public void Add(T item)
            {
                _items.Add(item);
            }
            
            public T GetItem(int index)
            {
                return _items[index];
            }
            
            public int Count => _items.Count;
            
            public IIterator<T> CreateIterator()
            {
                return new ConcreteIterator<T>(this);
            }
        }
        
        public class ConcreteIterator<T> : IIterator<T>
        {
            private readonly ConcreteAggregate<T> _aggregate;
            private int _current = 0;
            
            public ConcreteIterator(ConcreteAggregate<T> aggregate)
            {
                _aggregate = aggregate;
            }
            
            public T First()
            {
                return _aggregate.GetItem(0);
            }
            
            public T Next()
            {
                T item = default;
                if (_current < _aggregate.Count - 1)
                {
                    _current++;
                    item = _aggregate.GetItem(_current);
                }
                
                return item;
            }
            
            public bool IsDone()
            {
                return _current >= _aggregate.Count - 1;
            }
            
            public T CurrentItem()
            {
                return _aggregate.GetItem(_current);
            }
        }
    }
    
    // ===== PATRÓN MEDIATOR =====
    namespace MediatorPattern
    {
        public abstract class Colleague
        {
            protected IMediator _mediator;
            
            public Colleague(IMediator mediator)
            {
                _mediator = mediator;
            }
            
            public abstract void Send(string message);
            public abstract void Receive(string message);
        }
        
        public interface IMediator
        {
            void Send(string message, Colleague colleague);
        }
        
        public class ConcreteMediator : IMediator
        {
            private readonly List<Colleague> _colleagues = new();
            
            public void AddColleague(Colleague colleague)
            {
                _colleagues.Add(colleague);
            }
            
            public void Send(string message, Colleague colleague)
            {
                foreach (var col in _colleagues)
                {
                    if (col != colleague)
                    {
                        col.Receive(message);
                    }
                }
            }
        }
        
        public class ConcreteColleagueA : Colleague
        {
            public ConcreteColleagueA(IMediator mediator) : base(mediator) { }
            
            public override void Send(string message)
            {
                Console.WriteLine($"ColleagueA sends: {message}");
                _mediator.Send(message, this);
            }
            
            public override void Receive(string message)
            {
                Console.WriteLine($"ColleagueA receives: {message}");
            }
        }
        
        public class ConcreteColleagueB : Colleague
        {
            public ConcreteColleagueB(IMediator mediator) : base(mediator) { }
            
            public override void Send(string message)
            {
                Console.WriteLine($"ColleagueB sends: {message}");
                _mediator.Send(message, this);
            }
            
            public override void Receive(string message)
            {
                Console.WriteLine($"ColleagueB receives: {message}");
            }
        }
    }
    
    // ===== PATRÓN MEMENTO =====
    namespace MementoPattern
    {
        public class Memento
        {
            public string State { get; }
            
            public Memento(string state)
            {
                State = state;
            }
        }
        
        public class Originator
        {
            private string _state;
            
            public string State
            {
                get => _state;
                set
                {
                    _state = value;
                    Console.WriteLine($"State set to: {_state}");
                }
            }
            
            public Memento CreateMemento()
            {
                return new Memento(_state);
            }
            
            public void SetMemento(Memento memento)
            {
                _state = memento.State;
                Console.WriteLine($"State restored to: {_state}");
            }
        }
        
        public class Caretaker
        {
            private readonly List<Memento> _mementos = new();
            
            public void AddMemento(Memento memento)
            {
                _mementos.Add(memento);
            }
            
            public Memento GetMemento(int index)
            {
                return _mementos[index];
            }
        }
    }
    
    // ===== PATRÓN OBSERVER =====
    namespace ObserverPattern
    {
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
        
        public class ConcreteSubject : ISubject
        {
            private readonly List<IObserver> _observers = new();
            private string _state;
            
            public string State
            {
                get => _state;
                set
                {
                    _state = value;
                    Notify($"State changed to: {_state}");
                }
            }
            
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
        }
        
        public class ConcreteObserverA : IObserver
        {
            public void Update(string message)
            {
                Console.WriteLine($"ObserverA received: {message}");
            }
        }
        
        public class ConcreteObserverB : IObserver
        {
            public void Update(string message)
            {
                Console.WriteLine($"ObserverB received: {message}");
            }
        }
    }
    
    // ===== PATRÓN STATE =====
    namespace StatePattern
    {
        public interface IState
        {
            void Handle(Context context);
        }
        
        public class Context
        {
            private IState _state;
            
            public Context(IState state)
            {
                _state = state;
            }
            
            public IState State
            {
                get => _state;
                set
                {
                    _state = value;
                    Console.WriteLine($"State changed to: {_state.GetType().Name}");
                }
            }
            
            public void Request()
            {
                _state.Handle(this);
            }
        }
        
        public class ConcreteStateA : IState
        {
            public void Handle(Context context)
            {
                Console.WriteLine("ConcreteStateA handles request");
                context.State = new ConcreteStateB();
            }
        }
        
        public class ConcreteStateB : IState
        {
            public void Handle(Context context)
            {
                Console.WriteLine("ConcreteStateB handles request");
                context.State = new ConcreteStateA();
            }
        }
    }
    
    // ===== PATRÓN STRATEGY =====
    namespace StrategyPattern
    {
        public interface IStrategy
        {
            void AlgorithmInterface();
        }
        
        public class ConcreteStrategyA : IStrategy
        {
            public void AlgorithmInterface()
            {
                Console.WriteLine("ConcreteStrategyA algorithm");
            }
        }
        
        public class ConcreteStrategyB : IStrategy
        {
            public void AlgorithmInterface()
            {
                Console.WriteLine("ConcreteStrategyB algorithm");
            }
        }
        
        public class ConcreteStrategyC : IStrategy
        {
            public void AlgorithmInterface()
            {
                Console.WriteLine("ConcreteStrategyC algorithm");
            }
        }
        
        public class Context
        {
            private IStrategy _strategy;
            
            public Context(IStrategy strategy)
            {
                _strategy = strategy;
            }
            
            public void ContextInterface()
            {
                _strategy.AlgorithmInterface();
            }
            
            public void SetStrategy(IStrategy strategy)
            {
                _strategy = strategy;
            }
        }
    }
    
    // ===== PATRÓN TEMPLATE METHOD =====
    namespace TemplateMethodPattern
    {
        public abstract class AbstractClass
        {
            public void TemplateMethod()
            {
                PrimitiveOperation1();
                PrimitiveOperation2();
                Hook();
            }
            
            protected abstract void PrimitiveOperation1();
            protected abstract void PrimitiveOperation2();
            
            protected virtual void Hook()
            {
                // Default implementation
            }
        }
        
        public class ConcreteClassA : AbstractClass
        {
            protected override void PrimitiveOperation1()
            {
                Console.WriteLine("ConcreteClassA.PrimitiveOperation1");
            }
            
            protected override void PrimitiveOperation2()
            {
                Console.WriteLine("ConcreteClassA.PrimitiveOperation2");
            }
        }
        
        public class ConcreteClassB : AbstractClass
        {
            protected override void PrimitiveOperation1()
            {
                Console.WriteLine("ConcreteClassB.PrimitiveOperation1");
            }
            
            protected override void PrimitiveOperation2()
            {
                Console.WriteLine("ConcreteClassB.PrimitiveOperation2");
            }
            
            protected override void Hook()
            {
                Console.WriteLine("ConcreteClassB.Hook");
            }
        }
    }
    
    // ===== PATRÓN VISITOR =====
    namespace VisitorPattern
    {
        public interface IVisitor
        {
            void VisitConcreteElementA(ConcreteElementA element);
            void VisitConcreteElementB(ConcreteElementB element);
        }
        
        public interface IElement
        {
            void Accept(IVisitor visitor);
        }
        
        public class ConcreteElementA : IElement
        {
            public void Accept(IVisitor visitor)
            {
                visitor.VisitConcreteElementA(this);
            }
            
            public string OperationA()
            {
                return "ConcreteElementA";
            }
        }
        
        public class ConcreteElementB : IElement
        {
            public void Accept(IVisitor visitor)
            {
                visitor.VisitConcreteElementB(this);
            }
            
            public string OperationB()
            {
                return "ConcreteElementB";
            }
        }
        
        public class ConcreteVisitorA : IVisitor
        {
            public void VisitConcreteElementA(ConcreteElementA element)
            {
                Console.WriteLine($"ConcreteVisitorA visits {element.OperationA()}");
            }
            
            public void VisitConcreteElementB(ConcreteElementB element)
            {
                Console.WriteLine($"ConcreteVisitorA visits {element.OperationB()}");
            }
        }
        
        public class ConcreteVisitorB : IVisitor
        {
            public void VisitConcreteElementA(ConcreteElementA element)
            {
                Console.WriteLine($"ConcreteVisitorB visits {element.OperationA()}");
            }
            
            public void VisitConcreteElementB(ConcreteElementB element)
            {
                Console.WriteLine($"ConcreteVisitorB visits {element.OperationB()}");
            }
        }
    }
}

// Uso de Patrones de Diseño Enterprise
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Patrones de Diseño Enterprise ===\n");
        
        Console.WriteLine("Los patrones implementados incluyen:");
        Console.WriteLine("1. Composite - Composición de objetos");
        Console.WriteLine("2. Decorator - Funcionalidad dinámica");
        Console.WriteLine("3. Facade - Interfaz simplificada");
        Console.WriteLine("4. Flyweight - Reutilización de objetos");
        Console.WriteLine("5. Proxy - Control de acceso");
        Console.WriteLine("6. Chain of Responsibility - Cadena de responsabilidades");
        Console.WriteLine("7. Command - Encapsulación de comandos");
        Console.WriteLine("8. Interpreter - Interpretación de expresiones");
        Console.WriteLine("9. Iterator - Iteración sobre colecciones");
        Console.WriteLine("10. Mediator - Comunicación entre objetos");
        Console.WriteLine("11. Memento - Restauración de estado");
        Console.WriteLine("12. Observer - Notificaciones de cambios");
        Console.WriteLine("13. State - Cambios de comportamiento");
        Console.WriteLine("14. Strategy - Algoritmos intercambiables");
        Console.WriteLine("15. Template Method - Esqueleto de algoritmo");
        Console.WriteLine("16. Visitor - Operaciones sobre objetos");
        
        Console.WriteLine("\nBeneficios de estos patrones:");
        Console.WriteLine("- Código reutilizable y mantenible");
        Console.WriteLine("- Flexibilidad en el diseño");
        Console.WriteLine("- Separación de responsabilidades");
        Console.WriteLine("- Escalabilidad del sistema");
        Console.WriteLine("- Facilidad de testing");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Implementar Patrón Composite
Crea una estructura jerárquica de archivos y directorios usando el patrón Composite.

### Ejercicio 2: Patrón Observer Personalizado
Implementa un sistema de notificaciones usando el patrón Observer.

### Ejercicio 3: Patrón Strategy Avanzado
Crea un sistema de algoritmos de ordenamiento usando el patrón Strategy.

## 🔍 Puntos Clave

1. **Composite** permite tratar objetos individuales y compuestos uniformemente
2. **Decorator** agrega funcionalidad dinámicamente sin modificar clases
3. **Facade** proporciona una interfaz simplificada a subsistemas complejos
4. **Observer** implementa notificaciones de cambios entre objetos
5. **Strategy** permite intercambiar algoritmos en tiempo de ejecución

## 📚 Recursos Adicionales

- [Design Patterns - Gang of Four](https://en.wikipedia.org/wiki/Design_Patterns)
- [Enterprise Integration Patterns](https://www.enterpriseintegrationpatterns.com/)
- [Pattern-Oriented Software Architecture](https://en.wikipedia.org/wiki/Pattern-Oriented_Software_Architecture)

---

**🎯 ¡Has completado la Clase 4! Ahora comprendes Patrones de Diseño Enterprise**

**📚 [Siguiente: Clase 5 - Arquitectura de Datos Avanzada](clase_5_arquitectura_datos_avanzada.md)**
