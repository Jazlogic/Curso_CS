# 🚀 Clase 10: Proyecto Final - Sistema de Biblioteca

## 📋 Información de la Clase

- **Módulo**: Mid Level 3 - Manejo de Excepciones y Generics
- **Duración**: 4 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar todas las clases anteriores del módulo

## 🎯 Objetivos de Aprendizaje

- Integrar todos los conceptos aprendidos en el módulo
- Implementar un sistema completo usando generics y manejo de excepciones
- Aplicar patrones de diseño genéricos
- Crear una aplicación funcional y robusta

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_manejo_excepciones.md) | Manejo de Excepciones | |
| [Clase 2](clase_2_generics_basicos.md) | Generics Básicos | |
| [Clase 3](clase_3_generics_avanzados.md) | Generics Avanzados | |
| [Clase 4](clase_4_colecciones_genericas.md) | Colecciones Genéricas | |
| [Clase 5](clase_5_interfaces_genericas.md) | Interfaces Genéricas | |
| [Clase 6](clase_6_restricciones_generics.md) | Restricciones de Generics | |
| [Clase 7](clase_7_generics_reflection.md) | Generics y Reflection | |
| [Clase 8](clase_8_generics_performance.md) | Generics y Performance | |
| [Clase 9](clase_9_patrones_generics.md) | Patrones con Generics | ← Anterior |
| **Clase 10** | **Proyecto Final: Sistema de Biblioteca** | ← Estás aquí |

**← [Volver al README del Módulo 3](../midLevel_3/README.md)**

---

## 📚 Descripción del Proyecto

### Sistema de Biblioteca con Generics y Manejo de Excepciones

Este proyecto integra todos los conceptos aprendidos en el módulo para crear un sistema de gestión de biblioteca completo, robusto y extensible.

## 🏗️ Arquitectura del Sistema

```csharp
// ===== SISTEMA DE BIBLIOTECA - IMPLEMENTACIÓN COMPLETA =====
namespace LibrarySystem
{
    // ===== MODELOS BASE =====
    namespace Models
    {
        public abstract class BaseEntity<TKey> where TKey : IEquatable<TKey>
        {
            public TKey Id { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? UpdatedAt { get; set; }
            public bool IsDeleted { get; set; }
        }
        
        public class Book : BaseEntity<int>
        {
            public string Title { get; set; }
            public string Author { get; set; }
            public string ISBN { get; set; }
            public int PublicationYear { get; set; }
            public string Category { get; set; }
            public int AvailableCopies { get; set; }
            public int TotalCopies { get; set; }
        }
        
        public class User : BaseEntity<int>
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public DateTime MembershipDate { get; set; }
            public bool IsActive { get; set; }
        }
        
        public class Loan : BaseEntity<int>
        {
            public int UserId { get; set; }
            public int BookId { get; set; }
            public DateTime LoanDate { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime? ReturnDate { get; set; }
            public bool IsOverdue => DateTime.UtcNow > DueDate && ReturnDate == null;
        }
    }
    
    // ===== EXCEPCIONES PERSONALIZADAS =====
    namespace Exceptions
    {
        public class BookNotFoundException : Exception
        {
            public int BookId { get; }
            public BookNotFoundException(int bookId) : base($"Book with ID {bookId} not found")
            {
                BookId = bookId;
            }
        }
        
        public class UserNotFoundException : Exception
        {
            public int UserId { get; }
            public UserNotFoundException(int userId) : base($"User with ID {userId} not found")
            {
                UserId = userId;
            }
        }
        
        public class BookNotAvailableException : Exception
        {
            public int BookId { get; }
            public BookNotAvailableException(int bookId) : base($"Book with ID {bookId} is not available")
            {
                BookId = bookId;
            }
        }
        
        public class LoanOverdueException : Exception
        {
            public int LoanId { get; }
            public int DaysOverdue { get; }
            public LoanOverdueException(int loanId, int daysOverdue) 
                : base($"Loan {loanId} is {daysOverdue} days overdue")
            {
                LoanId = loanId;
                DaysOverdue = daysOverdue;
            }
        }
    }
    
    // ===== REPOSITORIOS GENÉRICOS =====
    namespace Repositories
    {
        public interface IGenericRepository<TEntity, TKey> where TEntity : BaseEntity<TKey> where TKey : IEquatable<TKey>
        {
            Task<TEntity> GetByIdAsync(TKey id);
            Task<IEnumerable<TEntity>> GetAllAsync();
            Task<TEntity> AddAsync(TEntity entity);
            Task<TEntity> UpdateAsync(TEntity entity);
            Task<bool> DeleteAsync(TKey id);
            Task<bool> ExistsAsync(TKey id);
            Task<int> CountAsync();
        }
        
        public class InMemoryGenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey> 
            where TEntity : BaseEntity<TKey> 
            where TKey : IEquatable<TKey>
        {
            private readonly Dictionary<TKey, TEntity> _entities = new Dictionary<TKey, TEntity>();
            private readonly object _lock = new object();
            
            public async Task<TEntity> GetByIdAsync(TKey id)
            {
                await Task.Delay(1); // Simular operación asíncrona
                
                lock (_lock)
                {
                    if (_entities.TryGetValue(id, out var entity) && !entity.IsDeleted)
                        return entity;
                    
                    throw new KeyNotFoundException($"Entity with ID {id} not found");
                }
            }
            
            public async Task<IEnumerable<TEntity>> GetAllAsync()
            {
                await Task.Delay(1);
                
                lock (_lock)
                {
                    return _entities.Values.Where(e => !e.IsDeleted).ToList();
                }
            }
            
            public async Task<TEntity> AddAsync(TEntity entity)
            {
                await Task.Delay(1);
                
                lock (_lock)
                {
                    if (entity.Id.Equals(default(TKey)))
                    {
                        // Generar ID simple para demostración
                        var nextId = _entities.Count > 0 ? _entities.Keys.Max() : default(TKey);
                        if (nextId is int intId)
                            entity.Id = (TKey)(object)(intId + 1);
                    }
                    
                    _entities[entity.Id] = entity;
                    return entity;
                }
            }
            
            public async Task<TEntity> UpdateAsync(TEntity entity)
            {
                await Task.Delay(1);
                
                lock (_lock)
                {
                    if (!_entities.ContainsKey(entity.Id))
                        throw new KeyNotFoundException($"Entity with ID {entity.Id} not found");
                    
                    entity.UpdatedAt = DateTime.UtcNow;
                    _entities[entity.Id] = entity;
                    return entity;
                }
            }
            
            public async Task<bool> DeleteAsync(TKey id)
            {
                await Task.Delay(1);
                
                lock (_lock)
                {
                    if (_entities.TryGetValue(id, out var entity))
                    {
                        entity.IsDeleted = true;
                        entity.UpdatedAt = DateTime.UtcNow;
                        return true;
                    }
                    return false;
                }
            }
            
            public async Task<bool> ExistsAsync(TKey id)
            {
                await Task.Delay(1);
                
                lock (_lock)
                {
                    return _entities.TryGetValue(id, out var entity) && !entity.IsDeleted;
                }
            }
            
            public async Task<int> CountAsync()
            {
                await Task.Delay(1);
                
                lock (_lock)
                {
                    return _entities.Values.Count(e => !e.IsDeleted);
                }
            }
        }
    }
    
    // ===== SERVICIOS GENÉRICOS =====
    namespace Services
    {
        public interface IGenericService<TEntity, TKey> where TEntity : BaseEntity<TKey> where TKey : IEquatable<TKey>
        {
            Task<TEntity> GetByIdAsync(TKey id);
            Task<IEnumerable<TEntity>> GetAllAsync();
            Task<TEntity> CreateAsync(TEntity entity);
            Task<TEntity> UpdateAsync(TEntity entity);
            Task<bool> DeleteAsync(TKey id);
        }
        
        public abstract class GenericService<TEntity, TKey> : IGenericService<TEntity, TKey> 
            where TEntity : BaseEntity<TKey> 
            where TKey : IEquatable<TKey>
        {
            protected readonly IGenericRepository<TEntity, TKey> _repository;
            protected readonly ILogger _logger;
            
            protected GenericService(IGenericRepository<TEntity, TKey> repository, ILogger logger)
            {
                _repository = repository ?? throw new ArgumentNullException(nameof(repository));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }
            
            public virtual async Task<TEntity> GetByIdAsync(TKey id)
            {
                try
                {
                    _logger.LogInformation($"Getting entity with ID: {id}");
                    var entity = await _repository.GetByIdAsync(id);
                    _logger.LogInformation($"Successfully retrieved entity with ID: {id}");
                    return entity;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting entity with ID: {id}");
                    throw;
                }
            }
            
            public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
            {
                try
                {
                    _logger.LogInformation("Getting all entities");
                    var entities = await _repository.GetAllAsync();
                    _logger.LogInformation($"Successfully retrieved {entities.Count()} entities");
                    return entities;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting all entities");
                    throw;
                }
            }
            
            public virtual async Task<TEntity> CreateAsync(TEntity entity)
            {
                try
                {
                    _logger.LogInformation($"Creating new entity of type: {typeof(TEntity).Name}");
                    var createdEntity = await _repository.AddAsync(entity);
                    _logger.LogInformation($"Successfully created entity with ID: {createdEntity.Id}");
                    return createdEntity;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error creating entity of type: {typeof(TEntity).Name}");
                    throw;
                }
            }
            
            public virtual async Task<TEntity> UpdateAsync(TEntity entity)
            {
                try
                {
                    _logger.LogInformation($"Updating entity with ID: {entity.Id}");
                    var updatedEntity = await _repository.UpdateAsync(entity);
                    _logger.LogInformation($"Successfully updated entity with ID: {updatedEntity.Id}");
                    return updatedEntity;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating entity with ID: {entity.Id}");
                    throw;
                }
            }
            
            public virtual async Task<bool> DeleteAsync(TKey id)
            {
                try
                {
                    _logger.LogInformation($"Deleting entity with ID: {id}");
                    var result = await _repository.DeleteAsync(id);
                    _logger.LogInformation($"Successfully deleted entity with ID: {id}");
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deleting entity with ID: {id}");
                    throw;
                }
            }
        }
    }
    
    // ===== SERVICIOS ESPECÍFICOS =====
    namespace SpecificServices
    {
        public interface IBookService : IGenericService<Book, int>
        {
            Task<IEnumerable<Book>> GetAvailableBooksAsync();
            Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
            Task<bool> ReserveBookAsync(int bookId, int userId);
            Task<bool> ReturnBookAsync(int bookId, int userId);
        }
        
        public class BookService : GenericService<Book, int>, IBookService
        {
            private readonly IGenericRepository<Loan, int> _loanRepository;
            
            public BookService(
                IGenericRepository<Book, int> bookRepository,
                IGenericRepository<Loan, int> loanRepository,
                ILogger logger) : base(bookRepository, logger)
            {
                _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            }
            
            public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
            {
                try
                {
                    var allBooks = await GetAllAsync();
                    return allBooks.Where(b => b.AvailableCopies > 0);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting available books");
                    throw;
                }
            }
            
            public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(searchTerm))
                        return await GetAllAsync();
                    
                    var allBooks = await GetAllAsync();
                    return allBooks.Where(b => 
                        b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        b.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        b.ISBN.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        b.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error searching books with term: {searchTerm}");
                    throw;
                }
            }
            
            public async Task<bool> ReserveBookAsync(int bookId, int userId)
            {
                try
                {
                    var book = await GetByIdAsync(bookId);
                    
                    if (book.AvailableCopies <= 0)
                        throw new BookNotAvailableException(bookId);
                    
                    book.AvailableCopies--;
                    await UpdateAsync(book);
                    
                    var loan = new Loan
                    {
                        UserId = userId,
                        BookId = bookId,
                        LoanDate = DateTime.UtcNow,
                        DueDate = DateTime.UtcNow.AddDays(14)
                    };
                    
                    await _loanRepository.AddAsync(loan);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error reserving book {bookId} for user {userId}");
                    throw;
                }
            }
            
            public async Task<bool> ReturnBookAsync(int bookId, int userId)
            {
                try
                {
                    var allLoans = await _loanRepository.GetAllAsync();
                    var loan = allLoans.FirstOrDefault(l => l.BookId == bookId && l.UserId == userId && l.ReturnDate == null);
                    
                    if (loan == null)
                        return false;
                    
                    loan.ReturnDate = DateTime.UtcNow;
                    await _loanRepository.UpdateAsync(loan);
                    
                    var book = await GetByIdAsync(bookId);
                    book.AvailableCopies++;
                    await UpdateAsync(book);
                    
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error returning book {bookId} for user {userId}");
                    throw;
                }
            }
        }
        
        public interface IUserService : IGenericService<User, int>
        {
            Task<IEnumerable<Loan>> GetUserLoansAsync(int userId);
            Task<IEnumerable<Loan>> GetOverdueLoansAsync(int userId);
            Task<bool> IsUserActiveAsync(int userId);
        }
        
        public class UserService : GenericService<User, int>, IUserService
        {
            private readonly IGenericRepository<Loan, int> _loanRepository;
            
            public UserService(
                IGenericRepository<User, int> userRepository,
                IGenericRepository<Loan, int> loanRepository,
                ILogger logger) : base(userRepository, logger)
            {
                _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            }
            
            public async Task<IEnumerable<Loan>> GetUserLoansAsync(int userId)
            {
                try
                {
                    var allLoans = await _loanRepository.GetAllAsync();
                    return allLoans.Where(l => l.UserId == userId && l.ReturnDate == null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting loans for user {userId}");
                    throw;
                }
            }
            
            public async Task<IEnumerable<Loan>> GetOverdueLoansAsync(int userId)
            {
                try
                {
                    var userLoans = await GetUserLoansAsync(userId);
                    return userLoans.Where(l => l.IsOverdue);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting overdue loans for user {userId}");
                    throw;
                }
            }
            
            public async Task<bool> IsUserActiveAsync(int userId)
            {
                try
                {
                    var user = await GetByIdAsync(userId);
                    return user.IsActive;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error checking if user {userId} is active");
                    throw;
                }
            }
        }
    }
    
    // ===== VALIDACIÓN GENÉRICA =====
    namespace Validation
    {
        public interface IGenericValidator<T>
        {
            Task<ValidationResult> ValidateAsync(T entity);
            Task<bool> IsValidAsync(T entity);
        }
        
        public class ValidationResult
        {
            public bool IsValid => !Errors.Any();
            public List<ValidationError> Errors { get; } = new List<ValidationError>();
            public List<ValidationError> Warnings { get; } = new List<ValidationError>();
            
            public void AddError(string propertyName, string message)
            {
                Errors.Add(new ValidationError { PropertyName = propertyName, Message = message });
            }
            
            public void AddWarning(string propertyName, string message)
            {
                Warnings.Add(new ValidationError { PropertyName = propertyName, Message = message });
            }
        }
        
        public class ValidationError
        {
            public string PropertyName { get; set; }
            public string Message { get; set; }
        }
        
        public class BookValidator : IGenericValidator<Book>
        {
            public async Task<ValidationResult> ValidateAsync(Book book)
            {
                var result = new ValidationResult();
                
                await Task.Delay(1); // Simular validación asíncrona
                
                if (string.IsNullOrWhiteSpace(book.Title))
                    result.AddError(nameof(book.Title), "Title is required");
                
                if (string.IsNullOrWhiteSpace(book.Author))
                    result.AddError(nameof(book.Author), "Author is required");
                
                if (string.IsNullOrWhiteSpace(book.ISBN))
                    result.AddError(nameof(book.ISBN), "ISBN is required");
                
                if (book.PublicationYear < 1800 || book.PublicationYear > DateTime.Now.Year)
                    result.AddError(nameof(book.PublicationYear), "Invalid publication year");
                
                if (book.AvailableCopies < 0)
                    result.AddError(nameof(book.AvailableCopies), "Available copies cannot be negative");
                
                if (book.TotalCopies < book.AvailableCopies)
                    result.AddError(nameof(book.TotalCopies), "Total copies cannot be less than available copies");
                
                return result;
            }
            
            public async Task<bool> IsValidAsync(Book book)
            {
                var result = await ValidateAsync(book);
                return result.IsValid;
            }
        }
    }
    
    // ===== CACHE GENÉRICO =====
    namespace Caching
    {
        public interface IGenericCache<TKey, TValue>
        {
            Task<bool> TryGetAsync(TKey key, out TValue value);
            Task SetAsync(TKey key, TValue value, TimeSpan? ttl = null);
            Task RemoveAsync(TKey key);
            Task ClearAsync();
        }
        
        public class InMemoryGenericCache<TKey, TValue> : IGenericCache<TKey, TValue>
        {
            private readonly Dictionary<TKey, CacheItem<TValue>> _cache = new Dictionary<TKey, CacheItem<TValue>>();
            private readonly object _lock = new object();
            
            public async Task<bool> TryGetAsync(TKey key, out TValue value)
            {
                await Task.Delay(1);
                
                lock (_lock)
                {
                    if (_cache.TryGetValue(key, out var item) && !item.IsExpired)
                    {
                        value = item.Value;
                        item.LastAccessed = DateTime.UtcNow;
                        return true;
                    }
                    
                    value = default(TValue);
                    return false;
                }
            }
            
            public async Task SetAsync(TKey key, TValue value, TimeSpan? ttl = null)
            {
                await Task.Delay(1);
                
                lock (_lock)
                {
                    _cache[key] = new CacheItem<TValue>
                    {
                        Value = value,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : null
                    };
                }
            }
            
            public async Task RemoveAsync(TKey key)
            {
                await Task.Delay(1);
                
                lock (_lock)
                {
                    _cache.Remove(key);
                }
            }
            
            public async Task ClearAsync()
            {
                await Task.Delay(1);
                
                lock (_lock)
                {
                    _cache.Clear();
                }
            }
        }
        
        public class CacheItem<T>
        {
            public T Value { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ExpiresAt { get; set; }
            public DateTime LastAccessed { get; set; }
            public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
        }
    }
    
    // ===== INTERFACES AUXILIARES =====
    namespace Interfaces
    {
        public interface ILogger
        {
            void LogInformation(string message);
            void LogWarning(string message);
            void LogError(Exception ex, string message);
        }
        
        public class ConsoleLogger : ILogger
        {
            public void LogInformation(string message)
            {
                Console.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss} - {message}");
            }
            
            public void LogWarning(string message)
            {
                Console.WriteLine($"[WARN] {DateTime.Now:HH:mm:ss} - {message}");
            }
            
            public void LogError(Exception ex, string message)
            {
                Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} - {message}: {ex.Message}");
            }
        }
    }
}

// Programa principal del Sistema de Biblioteca
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Sistema de Biblioteca - Proyecto Final ===\n");
        
        try
        {
            // Configurar dependencias
            var logger = new ConsoleLogger();
            var bookRepository = new InMemoryGenericRepository<Book, int>();
            var userRepository = new InMemoryGenericRepository<User, int>();
            var loanRepository = new InMemoryGenericRepository<Loan, int>();
            var bookService = new BookService(bookRepository, loanRepository, logger);
            var userService = new UserService(userRepository, loanRepository, logger);
            var bookValidator = new BookValidator();
            var cache = new InMemoryGenericCache<string, object>();
            
            // Crear datos de ejemplo
            var book1 = new Book
            {
                Title = "C# Programming",
                Author = "John Doe",
                ISBN = "1234567890",
                PublicationYear = 2023,
                Category = "Programming",
                AvailableCopies = 5,
                TotalCopies = 5
            };
            
            var user1 = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@email.com",
                Phone = "555-1234",
                MembershipDate = DateTime.UtcNow.AddDays(-30),
                IsActive = true
            };
            
            // Validar y crear entidades
            if (await bookValidator.IsValidAsync(book1))
            {
                var createdBook = await bookService.CreateAsync(book1);
                Console.WriteLine($"Book created: {createdBook.Title}");
            }
            
            var createdUser = await userService.CreateAsync(user1);
            Console.WriteLine($"User created: {createdUser.FirstName} {createdUser.LastName}");
            
            // Reservar libro
            var reservationResult = await bookService.ReserveBookAsync(1, 1);
            if (reservationResult)
            {
                Console.WriteLine("Book reserved successfully");
            }
            
            // Obtener libros disponibles
            var availableBooks = await bookService.GetAvailableBooksAsync();
            Console.WriteLine($"Available books: {availableBooks.Count()}");
            
            // Obtener préstamos del usuario
            var userLoans = await userService.GetUserLoansAsync(1);
            Console.WriteLine($"User loans: {userLoans.Count()}");
            
            // Buscar libros
            var searchResults = await bookService.SearchBooksAsync("C#");
            Console.WriteLine($"Search results: {searchResults.Count()}");
            
            Console.WriteLine("\n✅ Sistema de Biblioteca funcionando correctamente!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en el sistema: {ex.Message}");
        }
    }
}
```

## 🧪 Funcionalidades Implementadas

### ✅ Características Principales
- **Modelos genéricos** con entidades base
- **Repositorios genéricos** con operaciones CRUD
- **Servicios genéricos** con logging y manejo de excepciones
- **Validación genérica** para entidades
- **Cache genérico** para optimización
- **Manejo de excepciones** personalizado
- **Patrones de diseño** genéricos

### ✅ Operaciones del Sistema
- Gestión de libros (crear, leer, actualizar, eliminar)
- Gestión de usuarios
- Sistema de préstamos y devoluciones
- Búsqueda de libros
- Validación de datos
- Caching de operaciones frecuentes

## 🔍 Puntos Clave del Proyecto

1. **Arquitectura genérica** reutilizable
2. **Manejo robusto de excepciones**
3. **Patrones de diseño** implementados
4. **Validación genérica** de entidades
5. **Sistema de logging** integrado
6. **Cache genérico** para performance

## 📚 Recursos Adicionales

- [Microsoft Docs - Generic Collections](https://docs.microsoft.com/en-us/dotnet/standard/generics/)
- [Exception Handling Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/exceptions/)

---

**🎯 ¡Has completado el Módulo 3 completo! Ahora dominas Manejo de Excepciones y Generics**

**🏆 ¡Felicidades! Has completado exitosamente todo el módulo Mid Level 3**
