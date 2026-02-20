# Sistema de Gestión de Libros (C# + SQL Server + Docker)

Este proyecto es un sistema de información desarrollado en **C#** con una base de datos **Microsoft SQL Server**, diseñado para demostrar la gestión de entidades, relaciones y la integridad de transacciones (ACID).

## 1. Arquitectura del Sistema

El sistema utiliza una arquitectura de contenedores para asegurar que sea portable y fácil de desplegar.

- **Aplicación (App):** Interfaz de consola interactiva desarrollada en .NET 8.0.
- **Base de Datos (DB):** Motor MS SQL Server 2022 en un contenedor Docker.
- **Orquestación:** Docker Compose para la gestión de servicios y redes.

## 2. Modelo de Datos

### Entidades y Relaciones
- **Authors (Autores):** `Id`, `Name`, `Bio`.
- **Books (Libros):** `Id`, `Title`, `ISBN`, `Price`, `Stock`, `AuthorId`.
- **Relación:** 1:N (Un Autor -> Muchos Libros). Implementado con Llave Foránea.

### Inicialización Inteligente
El sistema localiza automáticamente el archivo `init.sql` en múltiples rutas y asegura que la base de datos `BookStoreDB` esté lista antes de permitir la interacción del usuario.

## 3. Características de la Interfaz Interactiva

El sistema ha sido evolucionado a una herramienta **CRUD Robusta**:
- **Validación de Datos:** Manejo de errores de formato en entradas numéricas (precios y stock) para evitar cierres inesperados.
- **Creación Dinámica de Autores:** Al agregar un libro, el usuario puede elegir un ID existente o escribir el nombre de un nuevo autor, el cual se registra automáticamente en la base de datos.
- **Menú de Navegación:** Opciones numeradas para Crear, Leer, Actualizar y Borrar registros en tiempo real.

## 4. Pruebas ACID Implementadas

### **A - Atomicidad (Atomicity)**
- **Prueba:** Transacción con fallo simulado y `Rollback`.
- **Justificación:** Garantiza que las operaciones financieras o de stock sean "todo o nada".

### **C - Consistencia (Consistency)**
- **Prueba:** Violación de integridad referencial.
- **Justificación:** Impide la entrada de datos que rompan las reglas de la base de datos.

### **I - Aislamiento (Isolation)**
- **Prueba:** Prevención de "lecturas sucias" (Dirty Reads).
- **Justificación:** Asegura que los cambios no confirmados no afecten a otros usuarios.

### **D - Durabilidad (Durability)**
- **Justificación:** Garantía de persistencia física tras el `Commit` (WAL).

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
*Documentación técnica actualizada el 20 de Febrero de 2026.*
