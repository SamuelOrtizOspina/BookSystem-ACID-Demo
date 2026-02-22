# Sistema de Gestión de Libros (C# + SQL Server + Docker)

Este proyecto es un sistema de información desarrollado en C# con una base de datos Microsoft SQL Server, diseñado para demostrar la gestión de entidades, relaciones y la integridad de transacciones (ACID).

## 1. Arquitectura del Sistema

El sistema utiliza una arquitectura modular para asegurar que sea portable, facil de gestionar y con una interfaz amigable.

- Interfaz Grafica (BookSystem.UI): Aplicacion profesional en Windows Forms que permite realizar CRUD visualmente sobre la base de datos de Docker.
- Aplicación de Consola (App): Interfaz interactiva para administracion basica y auditoria con sistema de logging.
- Base de Datos (DB): Motor MS SQL Server 2022 en un contenedor Docker con imagen profesional.
- Pruebas (App.Tests): Suite de pruebas unitarias en xUnit para asegurar la calidad del codigo y de los datos.

## 2. Modelo de Datos

### Entidades y Relaciones
- Authors (Autores): Id, Name, Bio.
- Books (Libros): Id, Title, ISBN, Price, Stock, AuthorId.
- Relación: 1:N (Un Autor -> Muchos Libros). Implementado con Llave Foranea.

### Inicialización Inteligente
El sistema localiza automáticamente el archivo init.sql en múltiples rutas para asegurar que la base de datos BookStoreDB esté lista antes de permitir la interacción.

## 3. Características Avanzadas y Cumplimiento Tareas

El sistema incluye funcionalidades para cumplir con los requerimientos profesionales:

### Interfaz Cliente (Tarea f)
- Interfaz Grafica: Se desarrollo una aplicacion en Windows Forms con un DataGridView para visualizar los datos en formato de tabla, ofreciendo una experiencia profesional.
- Registro de Auditoria: Cada accion critica se registra en system_log.txt.
- Prueba de Volumen: Posibilidad de insertar 100 libros de prueba de forma automatica para evaluar rendimiento.

### Pruebas de Datos (Tareas g, Manipulación y ACID)
- Pruebas de Conexión: El sistema valida el enlace con SQL Server al iniciar.
- Pruebas ACID: Implementadas tanto en consola como en interfaz grafica con cuadros de dialogo explicativos para Rollback y Commit.

## 4. Pruebas ACID Implementadas (Justificacion)

### A - Atomicidad (Atomicity)
- Prueba: Se modifica un precio y se fuerza un error simulado mediante MessageBox.
- Justificación: Se demuestra que SQL Server deshace el cambio si la operacion no termina, manteniendo el precio original.

### C - Consistencia (Consistency)
- Prueba: Insercion de datos invalidos.
- Justificación: Impide la entrada de datos que rompan las reglas de integridad referencial.

### I - Aislamiento (Isolation)
- Prueba: Prevención de lecturas sucias (Dirty Reads).
- Justificación: Asegura que los cambios no confirmados no afecten a otros usuarios concurrentes.

### D - Durabilidad (Durability)
- Justificación: Garantía de persistencia física de la base de datos tras el Commit mediante el registro de transacciones de SQL Server.

## 5. Instrucciones de Ejecución

### Despliegue de Base de Datos
```powershell
docker-compose up -d db
```

### Ejecución de Interfaz Gráfica
```powershell
dotnet run --project BookSystem.UI/BookSystem.UI.csproj
```

### Ejecución de Interfaz de Consola
```powershell
dotnet run --project App/BookSystem.csproj
```

---
Documentación técnica actualizada el 20 de Febrero de 2026.
Desarrollado por Samuel Ortiz.
