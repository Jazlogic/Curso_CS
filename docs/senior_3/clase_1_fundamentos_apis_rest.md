# 🚀 Clase 1: Fundamentos de APIs REST

## 🧭 Navegación
- **⬅️ Anterior**: [Módulo 9: Testing y TDD](../senior_2/README.md)
- **🏠 Inicio del Módulo**: [Módulo 10: APIs REST y Web APIs](README.md)
- **➡️ Siguiente**: [Clase 2: Configuración ASP.NET Core Web API](clase_2_configuracion_aspnet_core.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 📚 Descripción

En esta clase aprenderás los fundamentos de las APIs REST, sus principios arquitectónicos, verbos HTTP y cómo diseñar endpoints siguiendo las mejores prácticas.

## 🎯 Objetivos de Aprendizaje

- Comprender qué es REST y sus principios fundamentales
- Conocer los verbos HTTP y su uso correcto
- Aprender a diseñar URLs y endpoints RESTful
- Entender la importancia de los códigos de estado HTTP
- Aplicar las mejores prácticas en el diseño de APIs

## 📖 Contenido Teórico

### ¿Qué es REST?

REST (Representational State Transfer) es un estilo de arquitectura para sistemas distribuidos que se basa en el protocolo HTTP. Fue definido por Roy Fielding en su tesis doctoral en 2000.

**Características principales:**
- **Stateless**: Cada request debe contener toda la información necesaria
- **Client-Server**: Separación clara entre cliente y servidor
- **Cacheable**: Las respuestas deben ser cacheables
- **Uniform Interface**: Interfaz consistente y predecible
- **Layered System**: Arquitectura en capas
- **Code on Demand**: El servidor puede enviar código ejecutable al cliente

### Principios REST Fundamentales

#### 1. Recursos (Resources)
Los recursos son la abstracción fundamental en REST. Todo lo que se puede nombrar es un recurso.

```csharp
// Ejemplos de recursos
GET /api/users          // Colección de usuarios
GET /api/users/1        // Usuario específico
GET /api/users/1/orders // Órdenes de un usuario específico
```

#### 2. Representaciones
Los recursos pueden tener múltiples representaciones (JSON, XML, HTML, etc.).

```csharp
// El mismo recurso puede ser representado de diferentes formas
GET /api/users/1
Accept: application/json    // Respuesta en JSON
Accept: application/xml     // Respuesta en XML
Accept: text/html          // Respuesta en HTML
```

#### 3. Estado y Transiciones
REST se basa en el concepto de estado y transiciones entre estados.

```csharp
// Flujo típico de una API REST
POST /api/users           // Crear usuario (estado inicial)
GET /api/users/1          // Obtener usuario (estado actual)
PUT /api/users/1          // Actualizar usuario (nuevo estado)
DELETE /api/users/1       // Eliminar usuario (estado final)
```

### Verbos HTTP y su Uso

#### GET - Obtener Recursos
```csharp
// Obtener todos los usuarios
GET /api/users
// Respuesta: Lista de usuarios

// Obtener usuario específico
GET /api/users/1
// Respuesta: Usuario con ID 1

// Obtener usuario con filtros
GET /api/users?role=admin&active=true
// Respuesta: Usuarios administradores activos

// Obtener recursos anidados
GET /api/users/1/orders
// Respuesta: Órdenes del usuario 1
```

#### POST - Crear Nuevos Recursos
```csharp
// Crear nuevo usuario
POST /api/users
Content-Type: application/json

{
    "firstName": "Juan",
    "lastName": "Pérez",
    "email": "juan@example.com"
}

// Respuesta: Usuario creado con ID asignado
```

#### PUT - Actualizar Recursos Completos
```csharp
// Actualizar usuario completo
PUT /api/users/1
Content-Type: application/json

{
    "id": 1,
    "firstName": "Juan Carlos",
    "lastName": "Pérez",
    "email": "juancarlos@example.com",
    "role": "admin"
}

// Respuesta: Usuario actualizado completamente
```

#### PATCH - Actualizar Recursos Parcialmente
```csharp
// Actualizar solo algunos campos del usuario
PATCH /api/users/1
Content-Type: application/json

{
    "firstName": "Juan Carlos",
    "role": "admin"
}

// Respuesta: Solo los campos especificados se actualizan
```

#### DELETE - Eliminar Recursos
```csharp
// Eliminar usuario
DELETE /api/users/1

// Respuesta: Usuario eliminado (código 204 No Content)
```

### Códigos de Estado HTTP

#### Códigos 2xx - Éxito
```csharp
// 200 OK - Operación exitosa
GET /api/users/1 → 200 OK + datos del usuario

// 201 Created - Recurso creado exitosamente
POST /api/users → 201 Created + Location header

// 204 No Content - Operación exitosa sin contenido
DELETE /api/users/1 → 204 No Content
```

#### Códigos 4xx - Error del Cliente
```csharp
// 400 Bad Request - Solicitud mal formada
POST /api/users (datos inválidos) → 400 Bad Request

// 401 Unauthorized - No autenticado
GET /api/admin/users → 401 Unauthorized

// 403 Forbidden - No autorizado
DELETE /api/users/1 → 403 Forbidden

// 404 Not Found - Recurso no encontrado
GET /api/users/999 → 404 Not Found

// 409 Conflict - Conflicto con el estado actual
POST /api/users (email duplicado) → 409 Conflict
```

#### Códigos 5xx - Error del Servidor
```csharp
// 500 Internal Server Error - Error interno
GET /api/users → 500 Internal Server Error

// 503 Service Unavailable - Servicio no disponible
GET /api/users → 503 Service Unavailable
```

### Diseño de URLs RESTful

#### Convenciones de Nomenclatura
```csharp
// ✅ Correcto - URLs descriptivas y consistentes
GET /api/users                    // Colección de usuarios
GET /api/users/1                  // Usuario específico
GET /api/users/1/orders           // Órdenes de un usuario
GET /api/users/1/orders/5         // Orden específica de un usuario
GET /api/users/1/orders/5/items   // Items de una orden

// ❌ Incorrecto - URLs inconsistentes
GET /api/getUsers                 // Verbo en la URL
GET /api/user/1                   // Singular vs plural inconsistente
GET /api/users/getOrders/1        // Verbo en la URL
```

#### Filtros y Paginación
```csharp
// Filtros
GET /api/users?role=admin&active=true
GET /api/products?category=electronics&minPrice=100&maxPrice=500

// Paginación
GET /api/users?page=1&pageSize=10
GET /api/products?offset=20&limit=10

// Ordenamiento
GET /api/users?sortBy=lastName&sortOrder=asc
GET /api/products?sortBy=price&sortOrder=desc

// Búsqueda
GET /api/users?search=juan
GET /api/products?q=laptop
```

### Mejores Prácticas

#### 1. Consistencia en las URLs
```csharp
// Mantener consistencia en toda la API
GET    /api/users          // Obtener usuarios
POST   /api/users          // Crear usuario
GET    /api/users/{id}     // Obtener usuario específico
PUT    /api/users/{id}     // Actualizar usuario
DELETE /api/users/{id}     // Eliminar usuario

GET    /api/products       // Obtener productos
POST   /api/products       // Crear producto
GET    /api/products/{id}  // Obtener producto específico
PUT    /api/products/{id}  // Actualizar producto
DELETE /api/products/{id}  // Eliminar producto
```

#### 2. Uso Correcto de Verbos HTTP
```csharp
// ✅ Correcto - Usar verbos HTTP apropiados
GET    /api/users          // Obtener usuarios
POST   /api/users          // Crear usuario
PUT    /api/users/1        // Actualizar usuario completo
PATCH  /api/users/1        // Actualizar usuario parcialmente
DELETE /api/users/1        // Eliminar usuario

// ❌ Incorrecto - No usar verbos en URLs
GET    /api/getUsers
POST   /api/createUser
PUT    /api/updateUser/1
DELETE /api/deleteUser/1
```

#### 3. Manejo de Errores Consistente
```csharp
// Estructura consistente para errores
{
    "error": {
        "code": "USER_NOT_FOUND",
        "message": "El usuario no fue encontrado",
        "details": "No existe un usuario con el ID especificado",
        "timestamp": "2024-01-15T10:30:00Z"
    }
}

// Códigos de estado HTTP apropiados
400 Bad Request     // Datos de entrada inválidos
401 Unauthorized    // No autenticado
403 Forbidden       // No autorizado
404 Not Found       // Recurso no encontrado
409 Conflict        // Conflicto con estado actual
422 Unprocessable Entity // Datos válidos pero no procesables
500 Internal Server Error // Error interno del servidor
```

#### 4. Versionado de API
```csharp
// Versionado en la URL
GET /api/v1/users
GET /api/v2/users

// Versionado en headers
GET /api/users
Accept: application/vnd.company.app-v1+json

// Versionado en query parameters
GET /api/users?version=1
GET /api/users?api-version=1.0
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Diseño de URLs RESTful
Diseña las URLs para un sistema de blog que incluya:
- Usuarios
- Posts
- Comentarios
- Categorías
- Tags

### Ejercicio 2: Verbos HTTP Apropiados
Identifica el verbo HTTP correcto para cada operación:
- Obtener lista de productos
- Crear nuevo producto
- Actualizar precio de un producto
- Eliminar producto
- Obtener productos por categoría
- Buscar productos por nombre

### Ejercicio 3: Códigos de Estado HTTP
Para cada escenario, identifica el código de estado HTTP apropiado:
- Usuario creado exitosamente
- Usuario no encontrado
- Datos de entrada inválidos
- Usuario no autorizado para la operación
- Conflicto: email ya existe
- Error interno del servidor

### Ejercicio 4: Filtros y Paginación
Diseña URLs para:
- Obtener usuarios administradores activos
- Obtener productos de la categoría "electronics" con precio entre $100 y $500
- Obtener posts ordenados por fecha de publicación (más recientes primero)
- Obtener comentarios paginados (página 2, 10 por página)

## 📝 Quiz de Autoevaluación

1. ¿Cuáles son los 6 principios fundamentales de REST?
2. ¿Cuándo usarías PUT vs PATCH?
3. ¿Qué diferencia hay entre 401 Unauthorized y 403 Forbidden?
4. ¿Por qué es importante mantener consistencia en las URLs?
5. ¿Cuáles son las mejores prácticas para el versionado de APIs?

## 🔗 Enlaces Útiles

- [REST API Tutorial](https://restfulapi.net/)
- [HTTP Status Codes](https://httpstatuses.com/)
- [REST API Design Best Practices](https://blog.logrocket.com/rest-api-design-best-practices/)
- [Microsoft REST API Guidelines](https://github.com/microsoft/api-guidelines)

## 🚀 Siguiente Clase

En la siguiente clase aprenderás a configurar ASP.NET Core Web API, incluyendo la estructura del proyecto, configuración de servicios y middleware.

---

**💡 Consejo**: Practica diseñando URLs RESTful para diferentes dominios de negocio. La consistencia y claridad en el diseño de URLs es fundamental para crear APIs profesionales.
