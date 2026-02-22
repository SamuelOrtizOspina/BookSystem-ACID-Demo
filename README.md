# BookSystem-ACID-Demo

Este es un sistema de gestión de libros desarrollado en C# con MS SQL Server, diseñado para demostrar la implementación de arquitecturas relacionales, manejo de transacciones y cumplimiento de propiedades ACID en un entorno contenedorizado con Docker.

---

## Caracteristicas Principales

- Interfaz Grafica de Usuario (WinForms): Aplicacion de escritorio con tabla visual para gestion de inventario y pruebas ACID interactivas.
- Interfaz de Consola Profesional: Menu interactivo con sistema de robustez para prevenir cierres inesperados por errores de entrada.
- Registro de Auditoria (Logging): Todas las acciones criticas se registran en un archivo system_log.txt para seguimiento y seguridad.
- Prueba de Volumen (Stress Test): Funcion integrada para insertar 100 registros en lote para validar el rendimiento del motor SQL.
- Carga de Autores Inteligente: Permite seleccionar autores existentes o registrar uno nuevo simplemente escribiendo su nombre.
- Full Stack Dockerized: Base de datos orquestada con Docker Compose para asegurar portabilidad absoluta.
- Auto-Provisioning: Script SQL automatico para la creacion de esquemas y seeding de datos iniciales.

---

## Stack Tecnologico

- Lenguajes: C# (.NET 8.0 / .NET Core)
- Interfaz Grafica: Windows Forms (WinForms)
- Base de Datos: Microsoft SQL Server 2022 (Docker Image)
- Libreria de Datos: Microsoft.Data.SqlClient
- Infraestructura: Docker / Docker Compose
- Pruebas Unitarias: xUnit

---

## Modelo de Datos (DER)

El sistema gestiona una relacion Uno a Muchos (1:N) con integridad referencial:

- Authors: Id, Name, Bio
- Books: Id, Title, ISBN, Price, Stock, AuthorId (FK)

El script init.sql asegura que la base de datos BookStoreDB se cree con las restricciones adecuadas para evitar datos huerfanos.

---

## Pruebas ACID (Justificacion)

Este proyecto incluye una suite de pruebas para garantizar la fiabilidad absoluta de los datos:

1. Atomicidad (A): Valida que las transacciones se completen totalmente o se reviertan ante un fallo (Rollback). Implementado con cuadros de dialogo interactivos en la version grafica.
2. Consistencia (C): Asegura que las reglas de negocio (como Llaves Foraneas) se respeten, bloqueando datos invalidos en el motor SQL.
3. Aislamiento (I): Previene "lecturas sucias" (Dirty Reads) mediante el control de concurrencia de SQL Server.
4. Durabilidad (D): Garantiza que una vez confirmado (Commit), el dato persiste fisicamente incluso ante fallos del sistema.

---

## Como Ejecutar

### Requisito Previo (Base de Datos)

Asegurate de que Docker este corriendo y ejecuta:
```powershell
docker-compose up -d db
```

### Opcion 1: Interfaz Grafica (Recomendado para evaluacion)

Para abrir la ventana de escritorio con tabla visual:
```powershell
dotnet run --project BookSystem.UI/BookSystem.UI.csproj
```

### Opcion 2: Interfaz de Consola

Para interactuar mediante el menu de terminal:
```powershell
dotnet run --project App/BookSystem.csproj
```

---

## Conexion Remota

Puedes conectar herramientas externas (Azure Data Studio, SSMS) usando:
- Server: localhost,1433
- User: sa
- Password: YourStrong!Passw0rd
- Database: BookStoreDB

---

## Documentacion
Para un analisis tecnico mas profundo sobre la arquitectura y cumplimiento de tareas, consulta el archivo [DOCUMENTATION.md](./DOCUMENTATION.md).

---
Desarrollado como proyecto practico de Sistemas de Información por Samuel Ortiz.
