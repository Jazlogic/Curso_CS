# 🚀 Clase 10: Proyecto Final - Sistema de Biblioteca

## 📋 Información de la Clase

- **Módulo**: Mid Level 4 - LINQ y Expresiones Lambda
- **Duración**: 3 horas
- **Nivel**: Avanzado
- **Prerrequisitos**: Todas las clases anteriores del módulo

## 🎯 Objetivos de Aprendizaje

- Integrar todos los conceptos de LINQ y expresiones lambda
- Crear un sistema completo de gestión de biblioteca
- Implementar consultas complejas y optimizadas
- Aplicar patrones de diseño con LINQ

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_expresiones_lambda_avanzadas.md) | Expresiones Lambda Avanzadas | |
| [Clase 2](clase_2_operadores_linq_basicos.md) | Operadores LINQ Básicos | |
| [Clase 3](clase_3_operadores_linq_avanzados.md) | Operadores LINQ Avanzados | |
| [Clase 4](clase_4_linq_to_objects.md) | LINQ to Objects | |
| [Clase 5](clase_5_linq_to_xml.md) | LINQ to XML | |
| [Clase 6](clase_6_linq_to_sql.md) | LINQ to SQL | |
| [Clase 7](clase_7_linq_performance.md) | LINQ y Performance | |
| [Clase 8](clase_8_linq_optimization.md) | Optimización de LINQ | |
| [Clase 9](clase_9_linq_extension_methods.md) | Métodos de Extensión LINQ | ← Anterior |
| **Clase 10** | **Proyecto Final: Sistema de Biblioteca** | ← Estás aquí |

**← [Volver al README del Módulo 4](../midLevel_4/README.md)**

---

## 📚 Contenido Teórico

### 1. Sistema de Biblioteca Completo

Un sistema completo que integra todos los conceptos aprendidos en el módulo.

```csharp
// ===== SISTEMA DE BIBLIOTECA - IMPLEMENTACIÓN COMPLETA =====
using System.Xml.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace LibrarySystem
{
    // ===== MODELOS DE DATOS =====
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Category { get; set; }
        public int PublicationYear { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public decimal Price { get; set; }
        public List<string> Tags { get; set; }
        public DateTime AddedDate { get; set; }
        public bool IsActive { get; set; }
        
        public Book(int id, string title, string author, string isbn, string category, int year, int copies, decimal price)
        {
            Id = id;
            Title = title;
            Author = author;
            ISBN = isbn;
            Category = category;
            PublicationYear = year;
            TotalCopies = copies;
            AvailableCopies = copies;
            Price = price;
            Tags = new List<string>();
            AddedDate = DateTime.Now;
            IsActive = true;
        }
    }
    
    public class Member
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public int MaxBooksAllowed { get; set; }
        public decimal FineBalance { get; set; }
        
        public Member(int id, string name, string email, string phone)
        {
            Id = id;
            Name = name;
            Email = email;
            Phone = phone;
            RegistrationDate = DateTime.Now;
            ExpiryDate = DateTime.Now.AddYears(1);
            IsActive = true;
            MaxBooksAllowed = 5;
            FineBalance = 0;
        }
    }
    
    public class Loan
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal FineAmount { get; set; }
        public string Status { get; set; }
        
        public Loan(int id, int bookId, int memberId, int loanDays = 14)
        {
            Id = id;
            BookId = bookId;
            MemberId = memberId;
            LoanDate = DateTime.Now;
            DueDate = DateTime.Now.AddDays(loanDays);
            Status = "Active";
        }
    }
    
    public class Reservation
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
        
        public Reservation(int id, int bookId, int memberId)
        {
            Id = id;
            BookId = bookId;
            MemberId = memberId;
            ReservationDate = DateTime.Now;
            ExpiryDate = DateTime.Now.AddDays(7);
            Status = "Pending";
        }
    }
    
    // ===== SERVICIOS DE BIBLIOTECA =====
    public class LibraryService
    {
        private readonly List<Book> _books;
        private readonly List<Member> _members;
        private readonly List<Loan> _loans;
        private readonly List<Reservation> _reservations;
        
        public LibraryService()
        {
            _books = new List<Book>();
            _members = new List<Member>();
            _loans = new List<Loan>();
            _reservations = new List<Reservation>();
            InitializeSampleData();
        }
        
        // ===== OPERACIONES DE LIBROS =====
        public IEnumerable<Book> GetAllBooks() => _books.WhereActive();
        
        public IEnumerable<Book> SearchBooks(string searchTerm)
        {
            return _books.WhereActive()
                .Where(b => b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           b.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           b.ISBN.Contains(searchTerm) ||
                           b.Tags.Any(tag => tag.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
        }
        
        public IEnumerable<Book> GetBooksByCategory(string category)
        {
            return _books.WhereActive().WhereCategory(category);
        }
        
        public IEnumerable<Book> GetAvailableBooks()
        {
            return _books.WhereActive().Where(b => b.AvailableCopies > 0);
        }
        
        public IEnumerable<Book> GetOverdueBooks()
        {
            var overdueBookIds = _loans.Where(l => l.Status == "Active" && l.DueDate < DateTime.Now)
                .Select(l => l.BookId);
            
            return _books.Where(b => overdueBookIds.Contains(b.Id));
        }
        
        public IEnumerable<Book> GetPopularBooks(int count = 10)
        {
            return _loans.GroupBy(l => l.BookId)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => _books.First(b => b.Id == g.Key));
        }
        
        public BookStats GetBookStatistics()
        {
            var activeBooks = _books.WhereActive();
            
            return new BookStats
            {
                TotalBooks = _books.Count,
                ActiveBooks = activeBooks.Count(),
                TotalCopies = activeBooks.Sum(b => b.TotalCopies),
                AvailableCopies = activeBooks.Sum(b => b.AvailableCopies),
                Categories = activeBooks.Select(b => b.Category).Distinct().Count(),
                AveragePrice = activeBooks.Average(b => b.Price),
                BooksByYear = activeBooks.GroupBy(b => b.PublicationYear)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }
        
        // ===== OPERACIONES DE MIEMBROS =====
        public IEnumerable<Member> GetAllMembers() => _members.WhereActive();
        
        public IEnumerable<Member> GetActiveMembers()
        {
            return _members.Where(m => m.IsActive && m.ExpiryDate > DateTime.Now);
        }
        
        public IEnumerable<Member> GetMembersWithOverdueBooks()
        {
            var overdueMemberIds = _loans.Where(l => l.Status == "Active" && l.DueDate < DateTime.Now)
                .Select(l => l.MemberId)
                .Distinct();
            
            return _members.Where(m => overdueMemberIds.Contains(m.Id));
        }
        
        public IEnumerable<Member> GetTopBorrowers(int count = 10)
        {
            return _loans.GroupBy(l => l.MemberId)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => _members.First(m => m.Id == g.Key));
        }
        
        public MemberStats GetMemberStatistics()
        {
            var activeMembers = GetActiveMembers();
            
            return new MemberStats
            {
                TotalMembers = _members.Count,
                ActiveMembers = activeMembers.Count(),
                ExpiredMembers = _members.Count(m => m.ExpiryDate <= DateTime.Now),
                MembersWithFines = _members.Count(m => m.FineBalance > 0),
                AverageFineBalance = _members.Where(m => m.FineBalance > 0).Average(m => m.FineBalance),
                MembersByRegistrationYear = _members.GroupBy(m => m.RegistrationDate.Year)
                    .OrderByDescending(g => g.Count())
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }
        
        // ===== OPERACIONES DE PRÉSTAMOS =====
        public bool LoanBook(int bookId, int memberId)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId && b.IsActive);
            var member = _members.FirstOrDefault(m => m.Id == memberId && m.IsActive);
            
            if (book == null || member == null || book.AvailableCopies <= 0)
                return false;
            
            var activeLoans = _loans.Count(l => l.MemberId == memberId && l.Status == "Active");
            if (activeLoans >= member.MaxBooksAllowed)
                return false;
            
            book.AvailableCopies--;
            var loan = new Loan(_loans.Count + 1, bookId, memberId);
            _loans.Add(loan);
            
            return true;
        }
        
        public bool ReturnBook(int loanId)
        {
            var loan = _loans.FirstOrDefault(l => l.Id == loanId && l.Status == "Active");
            if (loan == null) return false;
            
            var book = _books.First(b => b.Id == loan.BookId);
            book.AvailableCopies++;
            
            loan.ReturnDate = DateTime.Now;
            loan.Status = "Returned";
            
            if (loan.ReturnDate > loan.DueDate)
            {
                var daysLate = (loan.ReturnDate.Value - loan.DueDate).Days;
                loan.FineAmount = daysLate * 0.50m; // $0.50 por día
                
                var member = _members.First(m => m.Id == loan.MemberId);
                member.FineBalance += loan.FineAmount;
            }
            
            return true;
        }
        
        public IEnumerable<Loan> GetActiveLoans()
        {
            return _loans.Where(l => l.Status == "Active");
        }
        
        public IEnumerable<Loan> GetOverdueLoans()
        {
            return _loans.Where(l => l.Status == "Active" && l.DueDate < DateTime.Now);
        }
        
        public IEnumerable<Loan> GetMemberLoans(int memberId)
        {
            return _loans.Where(l => l.MemberId == memberId).OrderByDescending(l => l.LoanDate);
        }
        
        public LoanStats GetLoanStatistics()
        {
            var activeLoans = GetActiveLoans();
            var overdueLoans = GetOverdueLoans();
            
            return new LoanStats
            {
                TotalLoans = _loans.Count,
                ActiveLoans = activeLoans.Count(),
                OverdueLoans = overdueLoans.Count(),
                TotalFines = _loans.Sum(l => l.FineAmount),
                AverageLoanDuration = _loans.Where(l => l.ReturnDate.HasValue)
                    .Average(l => (l.ReturnDate.Value - l.LoanDate).Days),
                LoansByMonth = _loans.GroupBy(l => new { l.LoanDate.Year, l.LoanDate.Month })
                    .OrderByDescending(g => g.Count())
                    .Take(12)
                    .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month}", g => g.Count())
            };
        }
        
        // ===== OPERACIONES DE RESERVAS =====
        public bool ReserveBook(int bookId, int memberId)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId && b.IsActive);
            var member = _members.FirstOrDefault(m => m.Id == memberId && m.IsActive);
            
            if (book == null || member == null)
                return false;
            
            var existingReservation = _reservations.FirstOrDefault(r => 
                r.BookId == bookId && r.MemberId == memberId && r.Status == "Pending");
            
            if (existingReservation != null)
                return false;
            
            var reservation = new Reservation(_reservations.Count + 1, bookId, memberId);
            _reservations.Add(reservation);
            
            return true;
        }
        
        public IEnumerable<Reservation> GetPendingReservations()
        {
            return _reservations.Where(r => r.Status == "Pending" && r.ExpiryDate > DateTime.Now);
        }
        
        public IEnumerable<Reservation> GetMemberReservations(int memberId)
        {
            return _reservations.Where(r => r.MemberId == memberId).OrderByDescending(r => r.ReservationDate);
        }
        
        // ===== CONSULTAS AVANZADAS =====
        public IEnumerable<BookRecommendation> GetBookRecommendations(int memberId, int count = 5)
        {
            var memberLoans = GetMemberLoans(memberId);
            var memberCategories = memberLoans.Select(l => _books.First(b => b.Id == l.BookId).Category)
                .GroupBy(c => c)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3);
            
            return _books.WhereActive()
                .Where(b => b.AvailableCopies > 0)
                .Where(b => memberCategories.Contains(b.Category))
                .OrderByDescending(b => b.PublicationYear)
                .Take(count)
                .Select(b => new BookRecommendation
                {
                    Book = b,
                    Reason = $"Basado en tu interés en {b.Category}",
                    Score = memberCategories.ToList().IndexOf(b.Category) + 1
                });
        }
        
        public IEnumerable<MemberActivity> GetMemberActivity(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            
            return _members.Select(m => new MemberActivity
            {
                Member = m,
                LoansCount = _loans.Count(l => l.MemberId == m.Id && l.LoanDate >= cutoffDate),
                ReservationsCount = _reservations.Count(r => r.MemberId == m.Id && r.ReservationDate >= cutoffDate),
                FineBalance = m.FineBalance,
                LastActivity = _loans.Where(l => l.MemberId == m.Id)
                    .OrderByDescending(l => l.LoanDate)
                    .Select(l => l.LoanDate)
                    .FirstOrDefault()
            })
            .OrderByDescending(ma => ma.LoansCount)
            .ThenByDescending(ma => ma.LastActivity);
        }
        
        public IEnumerable<CategoryAnalysis> GetCategoryAnalysis()
        {
            return _books.WhereActive()
                .GroupBy(b => b.Category)
                .Select(g => new CategoryAnalysis
                {
                    Category = g.Key,
                    BookCount = g.Count(),
                    TotalCopies = g.Sum(b => b.TotalCopies),
                    AvailableCopies = g.Sum(b => b.AvailableCopies),
                    AveragePrice = g.Average(b => b.Price),
                    LoanCount = _loans.Count(l => g.Any(b => b.Id == l.BookId)),
                    PopularityScore = _loans.Count(l => g.Any(b => b.Id == l.BookId)) / (double)g.Count()
                })
                .OrderByDescending(ca => ca.PopularityScore);
        }
        
        // ===== EXPORTACIÓN Y REPORTES =====
        public XDocument ExportBooksToXml()
        {
            var booksXml = new XElement("Books",
                _books.WhereActive().Select(b => new XElement("Book",
                    new XElement("Id", b.Id),
                    new XElement("Title", b.Title),
                    new XElement("Author", b.Author),
                    new XElement("ISBN", b.ISBN),
                    new XElement("Category", b.Category),
                    new XElement("PublicationYear", b.PublicationYear),
                    new XElement("AvailableCopies", b.AvailableCopies),
                    new XElement("Price", b.Price),
                    new XElement("Tags", string.Join(",", b.Tags))
                ))
            );
            
            return new XDocument(new XDeclaration("1.0", "utf-8", null), booksXml);
        }
        
        public string GenerateLoanReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== REPORTE DE PRÉSTAMOS ===");
            report.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            
            var loanStats = GetLoanStatistics();
            report.AppendLine($"Total de préstamos: {loanStats.TotalLoans}");
            report.AppendLine($"Préstamos activos: {loanStats.ActiveLoans}");
            report.AppendLine($"Préstamos vencidos: {loanStats.OverdueLoans}");
            report.AppendLine($"Multas totales: ${loanStats.TotalFines:F2}");
            report.AppendLine();
            
            var overdueLoans = GetOverdueLoans();
            if (overdueLoans.Any())
            {
                report.AppendLine("PRÉSTAMOS VENCIDOS:");
                foreach (var loan in overdueLoans)
                {
                    var book = _books.First(b => b.Id == loan.BookId);
                    var member = _members.First(m => m.Id == loan.MemberId);
                    var daysLate = (DateTime.Now - loan.DueDate).Days;
                    
                    report.AppendLine($"- {book.Title} prestado a {member.Name} (Vencido hace {daysLate} días)");
                }
            }
            
            return report.ToString();
        }
        
        // ===== INICIALIZACIÓN DE DATOS DE EJEMPLO =====
        private void InitializeSampleData()
        {
            // Agregar libros de ejemplo
            var books = new[]
            {
                new Book(1, "El Señor de los Anillos", "J.R.R. Tolkien", "978-0261103252", "Fantasía", 1954, 5, 29.99m),
                new Book(2, "1984", "George Orwell", "978-0451524935", "Ciencia Ficción", 1949, 3, 12.99m),
                new Book(3, "Cien años de soledad", "Gabriel García Márquez", "978-0307474728", "Literatura", 1967, 4, 15.99m),
                new Book(4, "Harry Potter y la piedra filosofal", "J.K. Rowling", "978-8478884452", "Fantasía", 1997, 8, 19.99m),
                new Book(5, "Don Quijote", "Miguel de Cervantes", "978-8420412146", "Literatura", 1605, 2, 24.99m),
                new Book(6, "El Hobbit", "J.R.R. Tolkien", "978-0261103283", "Fantasía", 1937, 6, 18.99m),
                new Book(7, "Dune", "Frank Herbert", "978-0441172719", "Ciencia Ficción", 1965, 3, 16.99m),
                new Book(8, "Los miserables", "Victor Hugo", "978-8420412146", "Literatura", 1862, 2, 22.99m),
                new Book(9, "El código Da Vinci", "Dan Brown", "978-8497592208", "Misterio", 2003, 7, 14.99m),
                new Book(10, "Los juegos del hambre", "Suzanne Collins", "978-8427202122", "Ciencia Ficción", 2008, 9, 13.99m)
            };
            
            foreach (var book in books)
            {
                book.Tags.AddRange(GetTagsForBook(book));
                _books.Add(book);
            }
            
            // Agregar miembros de ejemplo
            var members = new[]
            {
                new Member(1, "Juan Pérez", "juan@email.com", "555-0101"),
                new Member(2, "María García", "maria@email.com", "555-0102"),
                new Member(3, "Carlos López", "carlos@email.com", "555-0103"),
                new Member(4, "Ana Martínez", "ana@email.com", "555-0104"),
                new Member(5, "Luis Rodríguez", "luis@email.com", "555-0105")
            };
            
            foreach (var member in members)
            {
                _members.Add(member);
            }
            
            // Agregar préstamos de ejemplo
            var loans = new[]
            {
                new Loan(1, 1, 1) { LoanDate = DateTime.Now.AddDays(-10) },
                new Loan(2, 2, 2) { LoanDate = DateTime.Now.AddDays(-5) },
                new Loan(3, 4, 3) { LoanDate = DateTime.Now.AddDays(-15), DueDate = DateTime.Now.AddDays(-1) },
                new Loan(4, 6, 1) { LoanDate = DateTime.Now.AddDays(-3) },
                new Loan(5, 9, 4) { LoanDate = DateTime.Now.AddDays(-7) }
            };
            
            foreach (var loan in loans)
            {
                _loans.Add(loan);
                var book = _books.First(b => b.Id == loan.BookId);
                book.AvailableCopies--;
            }
            
            // Agregar reservas de ejemplo
            var reservations = new[]
            {
                new Reservation(1, 3, 2),
                new Reservation(2, 7, 5),
                new Reservation(3, 10, 1)
            };
            
            foreach (var reservation in reservations)
            {
                _reservations.Add(reservation);
            }
        }
        
        private string[] GetTagsForBook(Book book)
        {
            return book.Category switch
            {
                "Fantasía" => new[] { "aventura", "magia", "héroes" },
                "Ciencia Ficción" => new[] { "futuro", "tecnología", "espacio" },
                "Literatura" => new[] { "clásico", "drama", "historia" },
                "Misterio" => new[] { "suspenso", "investigación", "puzzle" },
                _ => new[] { "general" }
            };
        }
    }
    
    // ===== MÉTODOS DE EXTENSIÓN ESPECIALIZADOS =====
    public static class LibraryExtensions
    {
        public static IEnumerable<Book> WhereActive(this IEnumerable<Book> books)
        {
            return books.Where(b => b.IsActive);
        }
        
        public static IEnumerable<Book> WhereCategory(this IEnumerable<Book> books, string category)
        {
            return books.Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }
        
        public static IEnumerable<Book> WhereAvailable(this IEnumerable<Book> books)
        {
            return books.Where(b => b.AvailableCopies > 0);
        }
        
        public static IEnumerable<Book> WherePriceRange(this IEnumerable<Book> books, decimal minPrice, decimal maxPrice)
        {
            return books.Where(b => b.Price >= minPrice && b.Price <= maxPrice);
        }
        
        public static IEnumerable<Book> WhereYearRange(this IEnumerable<Book> books, int startYear, int endYear)
        {
            return books.Where(b => b.PublicationYear >= startYear && b.PublicationYear <= endYear);
        }
        
        public static IEnumerable<Book> WhereHasTag(this IEnumerable<Book> books, string tag)
        {
            return books.Where(b => b.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
        }
        
        public static IEnumerable<Member> WhereActive(this IEnumerable<Member> members)
        {
            return members.Where(m => m.IsActive && m.ExpiryDate > DateTime.Now);
        }
        
        public static IEnumerable<Member> WhereHasFines(this IEnumerable<Member> members)
        {
            return members.Where(m => m.FineBalance > 0);
        }
        
        public static IEnumerable<Member> WhereExpired(this IEnumerable<Member> members)
        {
            return members.Where(m => m.ExpiryDate <= DateTime.Now);
        }
    }
    
    // ===== CLASES DE ESTADÍSTICAS =====
    public class BookStats
    {
        public int TotalBooks { get; set; }
        public int ActiveBooks { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int Categories { get; set; }
        public decimal AveragePrice { get; set; }
        public Dictionary<int, int> BooksByYear { get; set; }
    }
    
    public class MemberStats
    {
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int ExpiredMembers { get; set; }
        public int MembersWithFines { get; set; }
        public decimal AverageFineBalance { get; set; }
        public Dictionary<int, int> MembersByRegistrationYear { get; set; }
    }
    
    public class LoanStats
    {
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public int OverdueLoans { get; set; }
        public decimal TotalFines { get; set; }
        public double AverageLoanDuration { get; set; }
        public Dictionary<string, int> LoansByMonth { get; set; }
    }
    
    public class BookRecommendation
    {
        public Book Book { get; set; }
        public string Reason { get; set; }
        public int Score { get; set; }
    }
    
    public class MemberActivity
    {
        public Member Member { get; set; }
        public int LoansCount { get; set; }
        public int ReservationsCount { get; set; }
        public decimal FineBalance { get; set; }
        public DateTime LastActivity { get; set; }
    }
    
    public class CategoryAnalysis
    {
        public string Category { get; set; }
        public int BookCount { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public decimal AveragePrice { get; set; }
        public int LoanCount { get; set; }
        public double PopularityScore { get; set; }
    }
}

// ===== PROGRAMA PRINCIPAL =====
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Sistema de Biblioteca - Proyecto Final ===\n");
        
        var library = new LibrarySystem.LibraryService();
        
        // Demostración de funcionalidades
        Console.WriteLine("1. BÚSQUEDA DE LIBROS:");
        var searchResults = library.SearchBooks("tolkien");
        foreach (var book in searchResults)
        {
            Console.WriteLine($"- {book.Title} por {book.Author}");
        }
        
        Console.WriteLine("\n2. ESTADÍSTICAS DE LIBROS:");
        var bookStats = library.GetBookStatistics();
        Console.WriteLine($"Total de libros: {bookStats.TotalBooks}");
        Console.WriteLine($"Libros activos: {bookStats.ActiveBooks}");
        Console.WriteLine($"Copias disponibles: {bookStats.AvailableCopies}");
        Console.WriteLine($"Precio promedio: ${bookStats.AveragePrice:F2}");
        
        Console.WriteLine("\n3. LIBROS POPULARES:");
        var popularBooks = library.GetPopularBooks(5);
        foreach (var book in popularBooks)
        {
            Console.WriteLine($"- {book.Title}");
        }
        
        Console.WriteLine("\n4. MIEMBROS CON LIBROS VENCIDOS:");
        var overdueMembers = library.GetMembersWithOverdueBooks();
        foreach (var member in overdueMembers)
        {
            Console.WriteLine($"- {member.Name} (${member.FineBalance:F2} en multas)");
        }
        
        Console.WriteLine("\n5. ANÁLISIS POR CATEGORÍA:");
        var categoryAnalysis = library.GetCategoryAnalysis();
        foreach (var analysis in categoryAnalysis)
        {
            Console.WriteLine($"- {analysis.Category}: {analysis.BookCount} libros, {analysis.LoanCount} préstamos");
        }
        
        Console.WriteLine("\n6. RECOMENDACIONES PARA MIEMBRO 1:");
        var recommendations = library.GetBookRecommendations(1, 3);
        foreach (var rec in recommendations)
        {
            Console.WriteLine($"- {rec.Book.Title} ({rec.Reason})");
        }
        
        Console.WriteLine("\n7. ACTIVIDAD DE MIEMBROS:");
        var memberActivity = library.GetMemberActivity(30);
        foreach (var activity in memberActivity.Take(3))
        {
            Console.WriteLine($"- {activity.Member.Name}: {activity.LoansCount} préstamos, {activity.ReservationsCount} reservas");
        }
        
        Console.WriteLine("\n8. REPORTE DE PRÉSTAMOS:");
        var loanReport = library.GenerateLoanReport();
        Console.WriteLine(loanReport);
        
        Console.WriteLine("\n✅ ¡Sistema de Biblioteca completado!");
        Console.WriteLine("Este proyecto integra todos los conceptos de LINQ y expresiones lambda aprendidos.");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Funcionalidades Adicionales
Implementa nuevas funcionalidades como:
- Sistema de notificaciones por email
- Reportes en PDF
- Dashboard web

### Ejercicio 2: Optimización
Optimiza las consultas más frecuentes usando:
- Consultas compiladas
- Caching
- Índices personalizados

### Ejercicio 3: Integración
Integra el sistema con:
- Base de datos SQL Server
- API REST
- Interfaz de usuario

## 🔍 Puntos Clave

1. **Integración completa** de todos los conceptos de LINQ
2. **Arquitectura modular** con servicios especializados
3. **Consultas complejas** con múltiples fuentes de datos
4. **Métodos de extensión** personalizados para el dominio
5. **Estadísticas y reportes** generados con LINQ
6. **Recomendaciones inteligentes** basadas en patrones
7. **Exportación de datos** a XML y otros formatos
8. **Gestión completa** de préstamos, reservas y multas

## 📚 Recursos Adicionales

- [Microsoft Docs - LINQ](https://docs.microsoft.com/en-us/dotnet/standard/linq/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)

---

**🎯 ¡Has completado el Módulo 4! Ahora dominas LINQ y Expresiones Lambda**

**📚 [Volver al README del Módulo 4](../midLevel_4/README.md)**
