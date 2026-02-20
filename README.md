# 📚 BookSystem-ACID-Demo

Este es un sistema de gestión de libros desarrollado en **C#** con **MS SQL Server**, diseñado para demostrar la implementación de arquitecturas relacionales, manejo de transacciones y cumplimiento de propiedades **ACID** en un entorno contenedorizado con **Docker**.

---

## 🚀 Características Principales

- **Full Stack Dockerized:** Aplicación y base de datos orquestadas con Docker Compose.
- **Auto-Provisioning:** Script SQL automático para la creación de esquemas y seeding de datos.
- **Relational Integrity:** Implementación de llaves foráneas y restricciones de datos.
- **ACID Testing Suite:** Módulo integrado para validar Atomicidad, Consistencia, Aislamiento y Durabilidad.
- **Dual Runtime:** Capacidad de ejecución local (.NET SDK) o mediante contenedores.

---

## 🛠️ Stack Tecnológico

- **Lenguaje:** C# (.NET 8.0)
- **Base de Datos:** Microsoft SQL Server 2022
- **Librería de Datos:** Microsoft.Data.SqlClient
- **Infraestructura:** Docker / Docker Compose
- **Control de Versiones:** Git

---

## 📂 Modelo de Datos (DER)

El sistema gestiona una relación **Uno a Muchos (1:N)**:

- **Authors:** `Id`, `Name`, `Bio`
- **Books:** `Id`, `Title`, `ISBN`, `Price`, `Stock`, `AuthorId (FK)`

El script `init.sql` asegura que la base de datos `BookStoreDB` se cree con las restricciones adecuadas de integridad referencial.

---

## 🧪 Pruebas ACID (Justificación)

Este proyecto incluye una suite de pruebas para garantizar la fiabilidad de los datos:

1.  **Atomicidad (A):** Valida que las transacciones se completen totalmente o se reviertan ante un fallo (Rollback).
2.  **Consistencia (C):** Asegura que las reglas de negocio (como Llaves Foráneas) se respeten, bloqueando datos inválidos.
3.  **Aislamiento (I):** Previene "lecturas sucias" (Dirty Reads) mediante el control de concurrencia.
4.  **Durabilidad (D):** Garantiza que una vez confirmado (Commit), el dato persiste físicamente gracias al diseño de SQL Server.

---

## ⚙️ Cómo Ejecutar

### Opción 1: Docker (Recomendado)

Solo necesitas tener Docker instalado. Ejecuta:

```powershell
docker-compose up -d --build
```

Para ver las pruebas en ejecución:
```powershell
docker logs csharp_book_app
```

### Opción 2: Ejecución Local

1.  Inicia solo la base de datos: `docker-compose up -d db`
2.  Entra en la carpeta del proyecto: `cd App`
3.  Ejecuta la aplicación: `dotnet run`

---

## 🔗 Conexión Remota

Puedes conectar herramientas externas (Azure Data Studio, SSMS) usando:
- **Server:** `localhost,1433`
- **User:** `sa`
- **Password:** `YourStrong!Passw0rd`
- **Database:** `BookStoreDB`

---

## 📝 Documentación
Para un análisis técnico más profundo, consulta el archivo [DOCUMENTATION.md](./DOCUMENTATION.md).

---
*Desarrollado como proyecto práctico de Sistemas de Información.*
