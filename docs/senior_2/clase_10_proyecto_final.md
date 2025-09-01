# 🚀 Clase 10: Proyecto Final - Sistema de Testing

## 📋 Información de la Clase

- **Módulo**: Senior Level 2 - Testing y TDD
- **Duración**: 3 horas
- **Nivel**: Avanzado
- **Prerrequisitos**: Todas las clases anteriores del módulo

## 🎯 Objetivos de Aprendizaje

- Integrar todos los conceptos de testing aprendidos
- Implementar un sistema completo de testing
- Aplicar TDD en un proyecto real
- Crear pruebas de calidad empresarial

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
| [Clase 7](clase_7_testing_asincrono.md) | Testing de Código Asíncrono | |
| [Clase 8](clase_8_testing_apis.md) | Testing de APIs | |
| [Clase 9](clase_9_testing_database.md) | Testing de Base de Datos | ← Anterior |
| **Clase 10** | **Proyecto Final: Sistema de Testing** | ← Estás aquí |

**← [Volver al README del Módulo 2](../senior_2/README.md)**

---

## 📚 Contenido Teórico

### 1. ¿Qué es el Proyecto Final?

El proyecto final integra todos los conceptos de testing aprendidos en un sistema completo de gestión de tareas con testing exhaustivo, aplicando TDD, mocking, testing asíncrono, y testing de base de datos.

### 2. Características del Sistema

- **Gestión de tareas** con usuarios y proyectos
- **Testing completo** de todas las capas
- **Arquitectura limpia** con separación de responsabilidades
- **Testing de integración** y unitario

```csharp
// ===== SISTEMA DE GESTIÓN DE TAREAS - PROYECTO FINAL =====
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace TaskManagementSystem
{
    // ===== MODELOS DE DOMINIO =====
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Project> OwnedProjects { get; set; } = new List<Project>();
        public List<Task> AssignedTasks { get; set; } = new List<Task>();
    }
    
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OwnerId { get; set; }
        public User Owner { get; set; }
        public ProjectStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<Task> Tasks { get; set; } = new List<Task>();
    }
    
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public int? AssignedUserId { get; set; }
        public User AssignedUser { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<TaskComment> Comments { get; set; } = new List<TaskComment>();
    }
    
    public class TaskComment
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public Task Task { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public enum ProjectStatus
    {
        Planning,
        Active,
        OnHold,
        Completed,
        Cancelled
    }
    
    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    public enum TaskStatus
    {
        Todo,
        InProgress,
        Review,
        Done,
        Cancelled
    }
    
    // ===== INTERFACES DE REPOSITORIOS =====
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    }
    
    public interface IProjectRepository
    {
        Task<Project> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Project project, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetByOwnerAsync(int ownerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status, CancellationToken cancellationToken = default);
    }
    
    public interface ITaskRepository
    {
        Task<Task> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Task>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Task> CreateAsync(Task task, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Task task, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Task>> GetByProjectAsync(int projectId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Task>> GetByAssigneeAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Task>> GetOverdueTasksAsync(CancellationToken cancellationToken = default);
    }
    
    // ===== SERVICIOS DE NEGOCIO =====
    public interface IUserService
    {
        Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
        Task<User> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);
        Task<User> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
        Task<bool> DeactivateUserAsync(int id, CancellationToken cancellationToken = default);
    }
    
    public interface IProjectService
    {
        Task<Project> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
        Task<Project> UpdateProjectAsync(int id, UpdateProjectRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteProjectAsync(int id, CancellationToken cancellationToken = default);
        Task<Project> GetProjectByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Project>> GetProjectsByOwnerAsync(int ownerId, CancellationToken cancellationToken = default);
        Task<bool> CompleteProjectAsync(int id, CancellationToken cancellationToken = default);
        Task<ProjectStatistics> GetProjectStatisticsAsync(int projectId, CancellationToken cancellationToken = default);
    }
    
    public interface ITaskService
    {
        Task<Task> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);
        Task<Task> UpdateTaskAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken = default);
        Task<Task> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Task>> GetTasksByProjectAsync(int projectId, CancellationToken cancellationToken = default);
        Task<bool> AssignTaskAsync(int taskId, int userId, CancellationToken cancellationToken = default);
        Task<bool> UpdateTaskStatusAsync(int taskId, TaskStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Task>> GetOverdueTasksAsync(CancellationToken cancellationToken = default);
        Task<TaskStatistics> GetTaskStatisticsAsync(int projectId, CancellationToken cancellationToken = default);
    }
    
    // ===== DTOs =====
    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    
    public class UpdateUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
    }
    
    public class CreateProjectRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int OwnerId { get; set; }
    }
    
    public class UpdateProjectRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ProjectStatus Status { get; set; }
    }
    
    public class CreateTaskRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public int? AssignedUserId { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
    }
    
    public class UpdateTaskRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int? AssignedUserId { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
    }
    
    // ===== ESTADÍSTICAS =====
    public class ProjectStatistics
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public decimal CompletionPercentage => TotalTasks > 0 ? (decimal)CompletedTasks / TotalTasks * 100 : 0;
    }
    
    public class TaskStatistics
    {
        public int TotalTasks { get; set; }
        public int TasksByPriority { get; set; }
        public int TasksByStatus { get; set; }
        public int OverdueTasks { get; set; }
        public decimal AverageCompletionTime { get; set; }
    }
    
    // ===== IMPLEMENTACIÓN DE SERVICIOS =====
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;
        
        public UserService(IUserRepository userRepository, ILogger logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }
        
        public async Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInfo($"Creating user: {request.Username}");
                
                // Validar entrada
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email))
                    throw new ArgumentException("Username and email are required");
                
                // Verificar si el usuario ya existe
                var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (existingUser != null)
                    throw new InvalidOperationException("User with this email already exists");
                
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                
                var createdUser = await _userRepository.CreateAsync(user, cancellationToken);
                _logger.LogInfo($"User created successfully: {createdUser.Id}");
                
                return createdUser;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating user", ex);
                throw;
            }
        }
        
        public async Task<User> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id, cancellationToken);
                if (user == null)
                    throw new ArgumentException("User not found");
                
                user.FirstName = request.FirstName ?? user.FirstName;
                user.LastName = request.LastName ?? user.LastName;
                user.IsActive = request.IsActive;
                
                var result = await _userRepository.UpdateAsync(user, cancellationToken);
                if (!result)
                    throw new InvalidOperationException("Failed to update user");
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user {id}", ex);
                throw;
            }
        }
        
        public async Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id, cancellationToken);
                if (user == null) return false;
                
                var result = await _userRepository.DeleteAsync(id, cancellationToken);
                if (result)
                    _logger.LogInfo($"User deleted: {id}");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user {id}", ex);
                return false;
            }
        }
        
        public async Task<User> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByIdAsync(id, cancellationToken);
        }
        
        public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetActiveUsersAsync(cancellationToken);
        }
        
        public async Task<bool> DeactivateUserAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id, cancellationToken);
                if (user == null) return false;
                
                user.IsActive = false;
                var result = await _userRepository.UpdateAsync(user, cancellationToken);
                
                if (result)
                    _logger.LogInfo($"User deactivated: {id}");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deactivating user {id}", ex);
                return false;
            }
        }
    }
    
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger _logger;
        
        public ProjectService(IProjectRepository projectRepository, ITaskRepository taskRepository, ILogger logger)
        {
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _logger = logger;
        }
        
        public async Task<Project> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                    throw new ArgumentException("Project name is required");
                
                var project = new Project
                {
                    Name = request.Name,
                    Description = request.Description,
                    OwnerId = request.OwnerId,
                    Status = ProjectStatus.Planning,
                    CreatedAt = DateTime.Now
                };
                
                var createdProject = await _projectRepository.CreateAsync(project, cancellationToken);
                _logger.LogInfo($"Project created: {createdProject.Id}");
                
                return createdProject;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating project", ex);
                throw;
            }
        }
        
        public async Task<bool> CompleteProjectAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
                if (project == null) return false;
                
                // Verificar que todas las tareas estén completadas
                var tasks = await _taskRepository.GetByProjectAsync(id, cancellationToken);
                var incompleteTasks = tasks.Where(t => t.Status != TaskStatus.Done);
                
                if (incompleteTasks.Any())
                    throw new InvalidOperationException("Cannot complete project with incomplete tasks");
                
                project.Status = ProjectStatus.Completed;
                project.CompletedAt = DateTime.Now;
                
                var result = await _projectRepository.UpdateAsync(project, cancellationToken);
                if (result)
                    _logger.LogInfo($"Project completed: {id}");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error completing project {id}", ex);
                return false;
            }
        }
        
        public async Task<ProjectStatistics> GetProjectStatisticsAsync(int projectId, CancellationToken cancellationToken = default)
        {
            var tasks = await _taskRepository.GetByProjectAsync(projectId, cancellationToken);
            
            return new ProjectStatistics
            {
                TotalTasks = tasks.Count(),
                CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Done),
                InProgressTasks = tasks.Count(t => t.Status == TaskStatus.InProgress),
                OverdueTasks = tasks.Count(t => t.DueDate.HasValue && t.DueDate < DateTime.Now && t.Status != TaskStatus.Done)
            };
        }
        
        // Implementar otros métodos...
        public async Task<Project> UpdateProjectAsync(int id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
        {
            // Implementación similar a UpdateUserAsync
            throw new NotImplementedException();
        }
        
        public async Task<bool> DeleteProjectAsync(int id, CancellationToken cancellationToken = default)
        {
            // Implementación similar a DeleteUserAsync
            throw new NotImplementedException();
        }
        
        public async Task<Project> GetProjectByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _projectRepository.GetByIdAsync(id, cancellationToken);
        }
        
        public async Task<IEnumerable<Project>> GetProjectsByOwnerAsync(int ownerId, CancellationToken cancellationToken = default)
        {
            return await _projectRepository.GetByOwnerAsync(ownerId, cancellationToken);
        }
    }
    
    // ===== INTERFACES ADICIONALES =====
    public interface ILogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception exception = null);
    }
    
    // ===== TESTING COMPLETO DEL SISTEMA =====
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ILogger> _mockLogger;
        private readonly UserService _userService;
        
        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger>();
            _userService = new UserService(_mockUserRepository.Object, _mockLogger.Object);
        }
        
        [Fact]
        public async Task CreateUser_WithValidRequest_ShouldSucceed()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            
            var expectedUser = new User
            {
                Id = 1,
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            
            _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);
            
            _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUser);
            
            // Act
            var result = await _userService.CreateUserAsync(request);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Username, result.Username);
            Assert.Equal(request.Email, result.Email);
            Assert.True(result.IsActive);
            
            _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockLogger.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains("User created successfully"))), Times.Once);
        }
        
        [Fact]
        public async Task CreateUser_WithDuplicateEmail_ShouldThrowException()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Username = "testuser",
                Email = "existing@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            
            var existingUser = new User { Email = request.Email };
            
            _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.CreateUserAsync(request));
            
            Assert.Contains("already exists", exception.Message);
            
            _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task UpdateUser_WithValidRequest_ShouldSucceed()
        {
            // Arrange
            var userId = 1;
            var existingUser = new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                FirstName = "Old",
                LastName = "Name",
                IsActive = true
            };
            
            var updateRequest = new UpdateUserRequest
            {
                FirstName = "New",
                LastName = "Name",
                IsActive = false
            };
            
            _mockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);
            
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            // Act
            var result = await _userService.UpdateUserAsync(userId, updateRequest);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateRequest.FirstName, result.FirstName);
            Assert.Equal(updateRequest.LastName, result.LastName);
            Assert.Equal(updateRequest.IsActive, result.IsActive);
        }
        
        [Fact]
        public async Task DeactivateUser_WithValidId_ShouldSucceed()
        {
            // Arrange
            var userId = 1;
            var user = new User
            {
                Id = userId,
                Username = "testuser",
                IsActive = true
            };
            
            _mockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            // Act
            var result = await _userService.DeactivateUserAsync(userId);
            
            // Assert
            Assert.True(result);
            Assert.False(user.IsActive);
            
            _mockLogger.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains("User deactivated"))), Times.Once);
        }
    }
    
    public class ProjectServiceTests
    {
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<ITaskRepository> _mockTaskRepository;
        private readonly Mock<ILogger> _mockLogger;
        private readonly ProjectService _projectService;
        
        public ProjectServiceTests()
        {
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockTaskRepository = new Mock<ITaskRepository>();
            _mockLogger = new Mock<ILogger>();
            _projectService = new ProjectService(_mockProjectRepository.Object, _mockTaskRepository.Object, _mockLogger.Object);
        }
        
        [Fact]
        public async Task CreateProject_WithValidRequest_ShouldSucceed()
        {
            // Arrange
            var request = new CreateProjectRequest
            {
                Name = "Test Project",
                Description = "A test project",
                OwnerId = 1
            };
            
            var expectedProject = new Project
            {
                Id = 1,
                Name = request.Name,
                Description = request.Description,
                OwnerId = request.OwnerId,
                Status = ProjectStatus.Planning,
                CreatedAt = DateTime.Now
            };
            
            _mockProjectRepository.Setup(r => r.CreateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedProject);
            
            // Act
            var result = await _projectService.CreateProjectAsync(request);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(ProjectStatus.Planning, result.Status);
            
            _mockLogger.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains("Project created"))), Times.Once);
        }
        
        [Fact]
        public async Task CompleteProject_WithAllTasksDone_ShouldSucceed()
        {
            // Arrange
            var projectId = 1;
            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                Status = ProjectStatus.Active
            };
            
            var tasks = new List<Task>
            {
                new Task { Id = 1, Status = TaskStatus.Done },
                new Task { Id = 2, Status = TaskStatus.Done }
            };
            
            _mockProjectRepository.Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);
            
            _mockTaskRepository.Setup(r => r.GetByProjectAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tasks);
            
            _mockProjectRepository.Setup(r => r.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            // Act
            var result = await _projectService.CompleteProjectAsync(projectId);
            
            // Assert
            Assert.True(result);
            Assert.Equal(ProjectStatus.Completed, project.Status);
            Assert.NotNull(project.CompletedAt);
        }
        
        [Fact]
        public async Task CompleteProject_WithIncompleteTasks_ShouldThrowException()
        {
            // Arrange
            var projectId = 1;
            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                Status = ProjectStatus.Active
            };
            
            var tasks = new List<Task>
            {
                new Task { Id = 1, Status = TaskStatus.Done },
                new Task { Id = 2, Status = TaskStatus.InProgress } // Tarea incompleta
            };
            
            _mockProjectRepository.Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);
            
            _mockTaskRepository.Setup(r => r.GetByProjectAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tasks);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _projectService.CompleteProjectAsync(projectId));
            
            Assert.Contains("incomplete tasks", exception.Message);
        }
        
        [Fact]
        public async Task GetProjectStatistics_ShouldReturnCorrectCounts()
        {
            // Arrange
            var projectId = 1;
            var tasks = new List<Task>
            {
                new Task { Id = 1, Status = TaskStatus.Done },
                new Task { Id = 2, Status = TaskStatus.InProgress },
                new Task { Id = 3, Status = TaskStatus.Todo, DueDate = DateTime.Now.AddDays(-1) }, // Overdue
                new Task { Id = 4, Status = TaskStatus.Done }
            };
            
            _mockTaskRepository.Setup(r => r.GetByProjectAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tasks);
            
            // Act
            var statistics = await _projectService.GetProjectStatisticsAsync(projectId);
            
            // Assert
            Assert.NotNull(statistics);
            Assert.Equal(4, statistics.TotalTasks);
            Assert.Equal(2, statistics.CompletedTasks);
            Assert.Equal(1, statistics.InProgressTasks);
            Assert.Equal(1, statistics.OverdueTasks);
            Assert.Equal(50.0m, statistics.CompletionPercentage);
        }
    }
}

// ===== DEMOSTRACIÓN DEL PROYECTO FINAL =====
public class ProjectFinalDemonstration
{
    public static async Task DemonstrateProjectFinal()
    {
        Console.WriteLine("=== Proyecto Final: Sistema de Testing - Clase 10 ===\n");
        
        Console.WriteLine("1. CREANDO SERVICIOS DEL SISTEMA:");
        var mockUserRepository = new Moq.Mock<TaskManagementSystem.IUserRepository>();
        var mockProjectRepository = new Moq.Mock<TaskManagementSystem.IProjectRepository>();
        var mockTaskRepository = new Moq.Mock<TaskManagementSystem.ITaskRepository>();
        var mockLogger = new Moq.Mock<TaskManagementSystem.ILogger>();
        
        Console.WriteLine("✅ Mocks creados exitosamente");
        
        Console.WriteLine("\n2. CONFIGURANDO COMPORTAMIENTOS:");
        mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskManagementSystem.User)null);
        
        mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<TaskManagementSystem.User>(), It.IsAny<CancellationToken>()))
            .Returns<TaskManagementSystem.User, CancellationToken>(async (user, token) =>
            {
                user.Id = 1;
                return user;
            });
        
        Console.WriteLine("✅ Comportamientos configurados");
        
        Console.WriteLine("\n3. CREANDO SERVICIOS:");
        var userService = new TaskManagementSystem.UserService(mockUserRepository.Object, mockLogger.Object);
        var projectService = new TaskManagementSystem.ProjectService(mockProjectRepository.Object, mockTaskRepository.Object, mockLogger.Object);
        
        Console.WriteLine("✅ Servicios creados");
        
        Console.WriteLine("\n4. PROBANDO FUNCIONALIDAD:");
        var createUserRequest = new TaskManagementSystem.CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        
        var user = await userService.CreateUserAsync(createUserRequest);
        Console.WriteLine($"✅ Usuario creado: {user.Username} (ID: {user.Id})");
        
        Console.WriteLine("\n5. VERIFICANDO LLAMADAS A MOCKS:");
        mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<TaskManagementSystem.User>(), It.IsAny<CancellationToken>()), Moq.Times.Once);
        mockLogger.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains("User created successfully"))), Moq.Times.Once);
        
        Console.WriteLine("✅ Llamadas verificadas correctamente");
        
        Console.WriteLine("\n🎯 ¡PROYECTO FINAL COMPLETADO!");
        Console.WriteLine("Has integrado exitosamente todos los conceptos de testing aprendidos:");
        Console.WriteLine("- Testing Unitario con xUnit");
        Console.WriteLine("- Mocking con Moq");
        Console.WriteLine("- Testing de servicios de negocio");
        Console.WriteLine("- Testing de validaciones y reglas de negocio");
        Console.WriteLine("- Arquitectura limpia y testing");
        Console.WriteLine("- Logging y manejo de errores");
    }
}

// Programa principal
public class Program
{
    public static async Task Main()
    {
        await ProjectFinalDemonstration.DemonstrateProjectFinal();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Implementación Completa
Completa la implementación de:
- TaskService con todas sus operaciones
- Testing de TaskService
- Validaciones de negocio adicionales

### Ejercicio 2: Testing de Integración
Implementa testing de integración para:
- Flujos completos de creación de proyectos
- Gestión de tareas en proyectos
- Estadísticas y reportes

### Ejercicio 3: Testing de Performance
Crea pruebas de performance para:
- Operaciones con grandes volúmenes de datos
- Consultas complejas de estadísticas
- Carga concurrente del sistema

## 🔍 Puntos Clave

1. **Integración completa** de todos los conceptos de testing
2. **Arquitectura limpia** con separación de responsabilidades
3. **Testing exhaustivo** de todas las capas del sistema
4. **Mocking efectivo** de dependencias externas
5. **Validación de reglas** de negocio
6. **Testing de escenarios** complejos
7. **Logging y manejo** de errores
8. **Testing de estadísticas** y reportes

## 📚 Recursos Adicionales

- [Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [Clean Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/)
- [Testing Patterns](https://martinfowler.com/articles/microservice-testing/)

---

**🎯 ¡FELICITACIONES! Has completado el Módulo 2: Testing y TDD**

**🚀 Has integrado exitosamente todos los conceptos de testing avanzado en C#**

**📚 [Volver al README del Módulo 2](../senior_2/README.md)**
