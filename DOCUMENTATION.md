# Sistema de Gestión de Libros (C# + SQL Server + Docker)

Este proyecto es un sistema de información desarrollado en C# con una base de datos Microsoft SQL Server, diseñado para demostrar la gestión de entidades, relaciones y la integridad de transacciones (ACID).

## 1. Arquitectura del Sistema

El sistema utiliza una arquitectura de contenedores para asegurar que sea portable y fácil de desplegar.

- Aplicación (App): Interfaz de consola interactiva con sistema de logging integrado.
- Base de Datos (DB): Motor MS SQL Server 2022 en un contenedor Docker.
- Orquestación: Docker Compose para la gestión de servicios y redes.

## 2. Modelo de Datos

### Entidades y Relaciones
- Authors (Autores): Id, Name, Bio.
- Books (Libros): Id, Title, ISBN, Price, Stock, AuthorId.
- Relación: 1:N (Un Autor -> Muchos Libros). Implementado con Llave Foránea.

### Inicialización Inteligente
El sistema localiza automáticamente el archivo init.sql en múltiples rutas y asegura que la base de datos BookStoreDB esté lista antes de permitir la interacción del usuario.

## 3. Características Avanzadas (Nivel Profesional)

El sistema incluye funcionalidades adicionales para entornos de producción:

### Registro de Auditoría (Logging)
- Ubicación: system_log.txt
- Funcionalidad: Se registra cada acción crítica (inicio de app, creación de autor, inserción de libro, cambio de precio, eliminación). Esto permite rastrear quién hizo qué y cuándo.

### Prueba de Volumen (Stress Testing)
- Funcionalidad: Permite insertar 100 libros de prueba de forma automática.
- Objetivo: Demostrar que el sistema mantiene su rendimiento y que la base de datos escala correctamente ante la inserción masiva de registros.

### Interfaz Robusta
- Validación de Datos: Manejo de errores de formato en entradas numéricas.
- Creación Dinámica: Al agregar un libro, el usuario puede registrar un nuevo autor al mismo tiempo.

## 4. Pruebas ACID Implementadas

### A - Atomicidad (Atomicity)
- Prueba: Transacción con fallo simulado y Rollback automático.

### C - Consistencia (Consistency)
- Prueba: Violación de integridad referencial.

### I - Aislamiento (Isolation)
- Prueba: Prevención de lecturas sucias (Dirty Reads).

### D - Durabilidad (Durability)
- Justificación: Garantía de persistencia física tras el Commit.

## 5. Instrucciones de Ejecución

### Despliegue con Docker
```powershell
docker-compose up -d --build
```

### Ejecución de la Interfaz
```powershell
dotnet run --project App/BookSystem.csproj
```

---
Documentación técnica actualizada el 20 de Febrero de 2026.
