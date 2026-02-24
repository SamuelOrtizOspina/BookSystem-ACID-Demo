# BookSystem-ACID-Demo

Este es un sistema de gestión de libros desarrollado en C# con MS SQL Server, diseñado para demostrar la implementación de arquitecturas relacionales, manejo de transacciones y cumplimiento de propiedades ACID en un entorno contenedorizado con Docker.

---

## Requisitos Previos

Para ejecutar este proyecto de forma correcta, es necesario tener instaladas las siguientes herramientas:

1. **Docker Desktop:** Necesario para el hospedaje del motor de base de datos SQL Server.
2. **.NET SDK (Versión 8.0 o 10.0):** Necesario para compilar y ejecutar la interfaz grafica y la aplicación de consola.
3. **Git:** Para la clonación y gestion del repositorio.

---

## Caracteristicas Principales

- Interfaz Grafica de Usuario (WinForms): Aplicacion de escritorio con un diseño moderno tipo Dashboard para gestion de inventario.
- Interfaz de Consola Profesional: Menu interactivo con sistema de auditoria y robustez de datos.
- Registro de Auditoria (Logging): Todas las acciones criticas se registran en system_log.txt para trazabilidad.
- Prueba de Volumen (Stress Test): Capacidad de insercion masiva de 100 registros para validacion de rendimiento.
- Full Stack Dockerized: Base de datos totalmente portable mediante contenedores.
- ACID Testing Suite: Herramientas integradas para validar Atomicidad, Consistencia, Aislamiento y Durabilidad.

---

## Stack Tecnologico

- Lenguajes: C# (.NET 8.0 / .NET 10.0)
- Interfaz Grafica: Windows Forms (WinForms) con diseño UX optimizado.
- Base de Datos: Microsoft SQL Server 2022 (Docker Image)
- Infraestructura: Docker / Docker Compose
- Pruebas Unitarias: xUnit

---

## Modelo de Datos (DER)

El sistema gestiona una relacion Uno a Muchos (1:N) con integridad referencial:

- Authors: Id, Name, Bio
- Books: Id, Title, ISBN, Price, Stock, AuthorId (FK)

El script init.sql asegura la creacion automatica de la base de datos y sus restricciones.

---

## Pruebas ACID (Justificacion)

Este proyecto valida la fiabilidad absoluta de los datos:

1. Atomicidad (A): Asegura que las transacciones se completen totalmente o se reviertan ante un fallo.
2. Consistencia (C): Bloquea datos invalidos que rompan las reglas de integridad del motor SQL.
3. Aislamiento (I): Previene interferencias entre usuarios concurrentes (Dirty Reads).
4. Durabilidad (D): Garantiza la persistencia fisica tras el Commit mediante el registro de transacciones.

---

## Como Ejecutar

### 1. Preparar la Base de Datos
Asegurate de que Docker Desktop este abierto y ejecuta:
```powershell
docker-compose up -d db
```

### 2. Ejecutar Interfaz Grafica (Recomendado)
Para abrir la ventana profesional:
```powershell
dotnet run --project BookSystem.UI/BookSystem.UI.csproj
```

### 3. Ejecutar Interfaz de Consola y Logs
```powershell
dotnet run --project App/BookSystem.csproj
```

---

## Conexion Remota

Datos de acceso para herramientas externas (Azure Data Studio, SSMS):
- Server: localhost,1433
- User: sa
- Password: YourStrong!Passw0rd
- Database: BookStoreDB
- Trust Server Certificate: True

---
Desarrollado como proyecto practico de Sistemas de Información por Samuel Ortiz.
