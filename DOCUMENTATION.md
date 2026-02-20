# Sistema de Gestión de Libros (C# + SQL Server + Docker)

Este proyecto es un sistema de información desarrollado en **C#** con una base de datos **Microsoft SQL Server**, diseñado para demostrar la gestión de entidades, relaciones y la integridad de transacciones (ACID).

## 1. Arquitectura del Sistema

El sistema utiliza una arquitectura de contenedores para asegurar que sea portable y fácil de desplegar.

- **Aplicación (App):** Desarrollada en .NET 8.0, se encarga de la lógica de negocio, inicialización de la base de datos y ejecución de pruebas.
- **Base de Datos (DB):** Motor MS SQL Server 2022 corriendo en un contenedor dedicado.
- **Orquestación:** Docker Compose gestiona la red y el ciclo de vida de ambos servicios.

## 2. Modelo de Datos

### Entidades y Relaciones
- **Authors (Autores):** Almacena `Id`, `Name` y `Bio`.
- **Books (Libros):** Almacena `Id`, `Title`, `ISBN`, `Price`, `Stock` y `AuthorId`.
- **Relación:** 1:N (Un Autor -> Muchos Libros). Implementado mediante una Constraint de Llave Foránea (`FK_Books_Authors`).

### Script de Inicialización (`init.sql`)
El sistema crea automáticamente la base de datos `BookStoreDB` y las tablas necesarias al iniciar, incluyendo datos semilla (seed data) para pruebas inmediatas.

## 3. Pruebas ACID Implementadas

El sistema incluye una suite de pruebas para validar las propiedades fundamentales de las bases de datos relacionales:

### **A - Atomicidad (Atomicity)**
- **Prueba:** Se inicia una transacción que intenta actualizar el precio de un libro y luego lanza una excepción intencional antes del `Commit`.
- **Justificación:** Se verifica que el sistema realice un `Rollback` automático. El precio del libro no cambia, demostrando que la operación es "todo o nada".

### **C - Consistencia (Consistency)**
- **Prueba:** Se intenta insertar un libro con un `AuthorId` que no existe en la tabla de Autores (ID 999).
- **Justificación:** SQL Server rechaza la inserción debido a la violación de integridad referencial (FK). Esto garantiza que no existan registros "huérfanos".

### **I - Aislamiento (Isolation)**
- **Prueba:** Se simulan dos transacciones concurrentes. La T1 modifica el stock pero no confirma (no hace commit). La T2 intenta leer ese stock.
- **Justificación:** El sistema previene "lecturas sucias" (Dirty Reads). La T2 lee el valor original hasta que la T1 confirme sus cambios, manteniendo la integridad visual de los datos.

### **D - Durabilidad (Durability)**
- **Justificación:** Una vez que la transacción recibe el `Commit`, SQL Server garantiza que los cambios persistan en el almacenamiento físico mediante el mecanismo de *Write-Ahead Logging* (WAL), incluso si hay un fallo de energía o reinicio del contenedor.

## 4. Instrucciones de Ejecución

### Requisitos
- Docker y Docker Compose instalados.
- (Opcional) SDK de .NET 8.0 para ejecución local.

### Despliegue con Docker (Recomendado)
Para levantar todo el sistema automáticamente:
```powershell
docker-compose up -d --build
```

### Ver Resultados de las Pruebas
Para ver la ejecución de las pruebas CRUD y ACID en tiempo real:
```powershell
docker logs csharp_book_app
```

## 5. Configuración de Conexión Remota
El servidor SQL está configurado para escuchar en el puerto **1433**. Se puede acceder desde herramientas externas (como Azure Data Studio o SSMS) utilizando:
- **Server:** localhost,1433
- **User:** sa
- **Password:** YourStrong!Passw0rd
- **TrustServerCertificate:** True

---
*Documentación generada para el Proyecto de Sistemas de Información.*
