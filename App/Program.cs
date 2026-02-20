using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace BookSystem
{
    class Program
    {
        const string Version = "2026-02-20-ROBUST";

        static string GetConnectionString(string dbName = "BookStoreDB")
        {
            string server = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
            return $"Server={server},1433;Database={dbName};User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Connect Timeout=30;";
        }

        static void Main(string[] args)
        {
            Console.Clear();
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
                Console.WriteLine("5. Salir");
                Console.Write("\nSeleccione una opción: ");

                string option = Console.ReadLine();
                try {
                    switch (option) {
                        case "1": ListBooks(); break;
                        case "2": AddBook(); break;
                        case "3": UpdatePrice(); break;
                        case "4": DeleteBook(); break;
                        case "5": exit = true; break;
                        default: Console.WriteLine("⚠️ Opción no válida."); break;
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"\n❌ ERROR: {ex.Message}");
                    Console.WriteLine("Presione cualquier tecla para continuar...");
                    Console.ReadKey();
                }
            }
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
        }

        static void AddBook()
        {
            Console.WriteLine("\n--- AGREGAR NUEVO LIBRO ---");
            Console.Write("Título del libro: ");
            string title = Console.ReadLine();
            Console.Write("ISBN: ");
            string isbn = Console.ReadLine();
            
            Console.Write("Precio (use '.' para decimales, ej: 25.50): ");
            if (!decimal.TryParse(Console.ReadLine().Replace(",", "."), out decimal price)) {
                Console.WriteLine("❌ Precio inválido. Debe ser un número.");
                return;
            }

            Console.Write("Stock inicial: ");
            if (!int.TryParse(Console.ReadLine(), out int stock)) {
                Console.WriteLine("❌ Stock inválido. Debe ser un número entero.");
                return;
            }
            
            // Mostrar autores para ayudar al usuario
            Console.WriteLine("\nAutores en el sistema:");
            int authorId = -1;
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (SqlDataReader r = new SqlCommand("SELECT Id, Name FROM Authors", conn).ExecuteReader())
                {
                    while (r.Read()) Console.WriteLine($"  ID: {r["Id"]} -> {r["Name"]}");
                }
                
                Console.Write("\nEscriba el ID del autor (o escriba el nombre de uno nuevo): ");
                string authorInput = Console.ReadLine();

                if (int.TryParse(authorInput, out int idExistente)) {
                    authorId = idExistente;
                } else {
                    // Crear nuevo autor si el usuario escribió un nombre
                    Console.WriteLine($"Creando nuevo autor: '{authorInput}'...");
                    string insertAuthor = "INSERT INTO Authors (Name, Bio) OUTPUT INSERTED.Id VALUES (@name, 'Autor agregado desde consola')";
                    using (SqlCommand cmd = new SqlCommand(insertAuthor, conn)) {
                        cmd.Parameters.AddWithValue("@name", authorInput);
                        authorId = (int)cmd.ExecuteScalar();
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
                    Console.WriteLine("✅ ¡Libro guardado con éxito!");
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
                    if (cmd.ExecuteNonQuery() > 0) Console.WriteLine("✅ Precio actualizado.");
                    else Console.WriteLine("❌ No se encontró el libro.");
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
                if (new SqlCommand($"DELETE FROM Books WHERE Id = {id}", conn).ExecuteNonQuery() > 0)
                    Console.WriteLine("✅ Libro eliminado.");
                else Console.WriteLine("❌ No se encontró el ID.");
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
            }
        }
    }
}
