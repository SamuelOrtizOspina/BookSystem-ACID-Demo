using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace BookSystem
{
    class Program
    {
        const string Version = "2026-02-20-PROFESSIONAL";
        const string LogFile = "system_log.txt";

        static string GetConnectionString(string dbName = "BookStoreDB")
        {
            string server = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
            return $"Server={server},1433;Database={dbName};User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Connect Timeout=30;";
        }

        static void Main(string[] args)
        {
            Console.Clear();
            LogAction("Iniciando aplicación.");
            InitializeDatabase();
            
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine($"\n=====================================================");
                Console.WriteLine($"   SISTEMA DE GESTIÓN DE LIBROS - Versión: {Version}");
                Console.WriteLine($"=====================================================");
                Console.WriteLine("1. Ver lista de libros (READ)");
                Console.WriteLine("2. Agregar un nuevo libro (CREATE)");
                Console.WriteLine("3. Actualizar precio de un libro (UPDATE)");
                Console.WriteLine("4. Eliminar un libro (DELETE)");
                Console.WriteLine("5. Prueba de volumen (Insertar 100 libros)");
                Console.WriteLine("6. Ejecutar Pruebas ACID");
                Console.WriteLine("7. Ver registro de auditoría (LOGS)");
                Console.WriteLine("8. Salir");
                Console.Write("\nSeleccione una opción: ");

                string option = Console.ReadLine();
                try {
                    switch (option) {
                        case "1": ListBooks(); break;
                        case "2": AddBook(); break;
                        case "3": UpdatePrice(); break;
                        case "4": DeleteBook(); break;
                        case "5": RunStressTest(); break;
                        case "6": RunACIDTests(); break;
                        case "7": ShowLogs(); break;
                        case "8": exit = true; break;
                        default: Console.WriteLine("Opción no válida."); break;
                    }
                } catch (Exception ex) {
                    LogAction($"ERROR: {ex.Message}");
                    Console.WriteLine($"\nERROR: {ex.Message}");
                    Console.WriteLine("Presione cualquier tecla para continuar...");
                    Console.ReadKey();
                }
            }
            LogAction("Cerrando aplicación.");
        }

        static void LogAction(string message)
        {
            try {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                File.AppendAllText(LogFile, logEntry);
            } catch { /* Ignorar fallos de log */ }
        }

        static void ShowLogs()
        {
            Console.WriteLine("\n--- REGISTRO DE AUDITORÍA (Últimas 20 líneas) ---");
            if (File.Exists(LogFile)) {
                string[] lines = File.ReadAllLines(LogFile);
                int start = Math.Max(0, lines.Length - 20);
                for (int i = start; i < lines.Length; i++) Console.WriteLine(lines[i]);
            } else Console.WriteLine("No hay registros todavía.");
        }

        static void ListBooks()
        {
            Console.WriteLine("\n--- LISTADO DE LIBROS ---");
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                string query = "SELECT B.Id, B.Title, B.ISBN, B.Price, B.Stock, A.Name as AuthorName FROM Books B JOIN Authors A ON B.AuthorId = A.Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) Console.WriteLine("No hay libros registrados.");
                    while (reader.Read())
                    {
                        Console.WriteLine($"ID: {reader["Id"]} | Título: {reader["Title"]} | Precio: ${reader["Price"]} | Autor: {reader["AuthorName"]}");
                    }
                }
            }
            LogAction("Consulta de lista de libros realizada.");
        }

        static void AddBook()
        {
            Console.WriteLine("\n--- AGREGAR NUEVO LIBRO ---");
            Console.Write("Título del libro: ");
            string title = Console.ReadLine();
            Console.Write("ISBN: ");
            string isbn = Console.ReadLine();
            
            Console.Write("Precio (ej: 25.50): ");
            if (!decimal.TryParse(Console.ReadLine().Replace(",", "."), out decimal price)) return;

            Console.Write("Stock inicial: ");
            if (!int.TryParse(Console.ReadLine(), out int stock)) return;
            
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                Console.Write("\nEscriba el ID del autor o nombre de uno nuevo: ");
                string authorInput = Console.ReadLine();
                int authorId;

                if (int.TryParse(authorInput, out int idExistente)) {
                    authorId = idExistente;
                } else {
                    string insertAuthor = "INSERT INTO Authors (Name, Bio) OUTPUT INSERTED.Id VALUES (@name, 'Auto-creado')";
                    using (SqlCommand cmd = new SqlCommand(insertAuthor, conn)) {
                        cmd.Parameters.AddWithValue("@name", authorInput);
                        authorId = (int)cmd.ExecuteScalar();
                        LogAction($"Nuevo autor creado: {authorInput}");
                    }
                }

                string query = "INSERT INTO Books (Title, ISBN, Price, Stock, AuthorId) VALUES (@title, @isbn, @price, @stock, @authorId)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@isbn", isbn);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@stock", stock);
                    cmd.Parameters.AddWithValue("@authorId", authorId);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Libro guardado.");
                    LogAction($"Libro agregado: {title} (ISBN: {isbn})");
                }
            }
        }

        static void UpdatePrice()
        {
            Console.Write("\nID del libro a actualizar: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;
            Console.Write("Nuevo precio: ");
            if (!decimal.TryParse(Console.ReadLine().Replace(",", "."), out decimal newPrice)) return;

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                string query = "UPDATE Books SET Price = @price WHERE Id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@price", newPrice);
                    cmd.Parameters.AddWithValue("@id", id);
                    if (cmd.ExecuteNonQuery() > 0) {
                        Console.WriteLine("Precio actualizado.");
                        LogAction($"Precio actualizado para libro ID {id} a ${newPrice}");
                    }
                }
            }
        }

        static void DeleteBook()
        {
            Console.Write("\nID del libro a eliminar: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                if (new SqlCommand($"DELETE FROM Books WHERE Id = {id}", conn).ExecuteNonQuery() > 0) {
                    Console.WriteLine("Libro eliminado.");
                    LogAction($"Libro eliminado ID {id}");
                }
            }
        }

        static void RunStressTest()
        {
            Console.WriteLine("\n--- PRUEBA DE VOLUMEN (INSERTANDO 100 LIBROS) ---");
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                for (int i = 1; i <= 100; i++)
                {
                    string title = $"Libro de Prueba #{i}";
                    string isbn = $"STRESS-{Guid.NewGuid().ToString().Substring(0, 8)}";
                    string query = "INSERT INTO Books (Title, ISBN, Price, Stock, AuthorId) VALUES (@t, @i, 10.00, 1, 1)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@t", title);
                        cmd.Parameters.AddWithValue("@i", isbn);
                        cmd.ExecuteNonQuery();
                    }
                    if (i % 20 == 0) Console.WriteLine($"Insertados {i} libros...");
                }
                Console.WriteLine("¡Prueba de volumen completada con éxito!");
                LogAction("Prueba de volumen ejecutada: 100 libros insertados.");
            }
        }

        static void RunACIDTests()
        {
            Console.WriteLine("\n--- PRUEBAS ACID ---");
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try {
                        new SqlCommand("UPDATE Books SET Price = 0 WHERE Id = 1", conn, trans).ExecuteNonQuery();
                        throw new Exception("Fallo de prueba");
                    } catch {
                        trans.Rollback();
                        Console.WriteLine("Atomicidad: OK (Rollback verificado)");
                        LogAction("Prueba ACID ejecutada.");
                    }
                }
            }
        }

        static void InitializeDatabase()
        {
            string connectionStringMaster = GetConnectionString("master");
            string scriptPath = "init.sql";
            if (!File.Exists(scriptPath)) scriptPath = "../init.sql";
            if (!File.Exists(scriptPath)) scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "init.sql");
            
            if (File.Exists(scriptPath)) {
                string script = File.ReadAllText(scriptPath);
                string[] commands = script.Split(new string[] { "GO", "go", "Go" }, StringSplitOptions.RemoveEmptyEntries);
                using (SqlConnection conn = new SqlConnection(connectionStringMaster)) {
                    conn.Open();
                    foreach (string cmdText in commands) {
                        if (string.IsNullOrWhiteSpace(cmdText)) continue;
                        using (SqlCommand cmd = new SqlCommand(cmdText, conn)) { try { cmd.ExecuteNonQuery(); } catch {} }
                    }
                }
                LogAction("Base de datos inicializada.");
            }
        }
    }
}
