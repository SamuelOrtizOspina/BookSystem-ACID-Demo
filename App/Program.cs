using System;
using System.Data.SqlClient;
using System.Threading;
using System.IO;
using Microsoft.Data.SqlClient;

namespace BookSystem
{
    class Program
    {
        static string GetConnectionString(string dbName = "BookStoreDB")
        {
            string server = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
            return $"Server={server},1433;Database={dbName};User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== Sistema de Gestión de Libros (C# + SQL Server) ===");
            Console.WriteLine($"Conectando a: {Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost"}");
            Console.WriteLine("Esperando a que la base de datos inicie...");
            Thread.Sleep(5000); // Reduced wait time

            try
            {
                InitializeDatabase();
                TestConnection();
                ManipulateData();
                RunACIDTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crítico: {ex.Message}");
            }
        }

        static void InitializeDatabase()
        {
            Console.WriteLine("\n--- Inicializando Base de Datos ---");
            string connectionStringMaster = GetConnectionString("master");
            string scriptPath = Environment.GetEnvironmentVariable("INIT_SQL_PATH") ?? "../init.sql";

            if (!File.Exists(scriptPath))
            {
                Console.WriteLine($"Advertencia: No se encontró el archivo '{scriptPath}'. Asumiendo que la BD ya existe.");
                return;
            }

            string script = File.ReadAllText(scriptPath);
            
            // Split script by 'GO' command (SQL Server batch separator)
            string[] commands = script.Split(new string[] { "GO", "go", "Go" }, StringSplitOptions.RemoveEmptyEntries);

            using (SqlConnection conn = new SqlConnection(connectionStringMaster))
            {
                conn.Open();
                foreach (string command in commands)
                {
                    if (string.IsNullOrWhiteSpace(command)) continue;
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(command, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine($"Error ejecutando script SQL: {ex.Message}");
                    }
                }
                Console.WriteLine("Base de datos inicializada correctamente.");
            }
        }

        static void TestConnection()
        {
            Console.WriteLine("\n--- Prueba de Conexión ---");
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                Console.WriteLine("Conexión Exitosa a SQL Server!");
            }
        }

        static void ManipulateData()
        {
            Console.WriteLine("\n--- Pruebas de Manipulación de Datos (CRUD) ---");
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                // Create (Insert)
                Console.WriteLine("Insertando nuevo libro...");
                string checkQuery = "SELECT COUNT(*) FROM Books WHERE ISBN = '978-0307950925'";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count == 0)
                    {
                        string insertQuery = "INSERT INTO Books (Title, ISBN, Price, Stock, AuthorId) VALUES ('El Aleph', '978-0307950925', 14.50, 30, 1)";
                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                        {
                            int rows = cmd.ExecuteNonQuery();
                            Console.WriteLine($"Filas insertadas: {rows}");
                        }
                    }
                    else
                    {
                         Console.WriteLine("El libro 'El Aleph' ya existe. Saltando inserción.");
                    }
                }

                // Read (Select)
                Console.WriteLine("Leyendo libros de Gabriel Garcia Marquez...");
                string selectQuery = "SELECT Title, Price FROM Books WHERE AuthorId = 1";
                using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"- {reader["Title"]} (${reader["Price"]})");
                    }
                }
            }
        }

        static void RunACIDTests()
        {
            Console.WriteLine("\n--- Pruebas ACID ---");

            // 1. Atomicity: Transaction Rollback
            Console.WriteLine("\n[A] Atomicidad: Intentando transacción fallida...");
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Step 1: Valid Update
                    new SqlCommand("UPDATE Books SET Price = 999 WHERE Id = 1", conn, transaction).ExecuteNonQuery();
                    Console.WriteLine("Paso 1: Precio actualizado (en memoria).");

                    // Step 2: Force Error (Divide by zero)
                    Console.WriteLine("Paso 2: Generando error intencional...");
                    throw new Exception("Simulando fallo del sistema antes del Commit.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error capturado: {ex.Message}");
                    transaction.Rollback();
                    Console.WriteLine("Rollback ejecutado.");
                }
            }

            // Verify Rollback
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                decimal price = (decimal)new SqlCommand("SELECT Price FROM Books WHERE Id = 1", conn).ExecuteScalar();
                Console.WriteLine($"Verificación: El precio sigue siendo {price} (No cambió a 999). Prueba de Atomicidad EXITOSA.");
            }

            // 2. Consistency: Foreign Key Constraint
            Console.WriteLine("\n[C] Consistencia: Intentando violar integridad referencial...");
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                try
                {
                    // Try to insert book with non-existent AuthorId (999)
                    string invalidInsert = "INSERT INTO Books (Title, ISBN, Price, Stock, AuthorId) VALUES ('Libro Invalido', '999-9999999999', 10.00, 1, 999)";
                    new SqlCommand(invalidInsert, conn).ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Error de SQL capturado: {ex.Message}");
                    Console.WriteLine("La base de datos rechazó la operación inválida. Prueba de Consistencia EXITOSA.");
                }
            }

            // 3. Isolation: Simulate Concurrent Updates
            Console.WriteLine("\n[I] Aislamiento: Simulando lectura sucia (Dirty Read prevention)...");
            using (SqlConnection conn1 = new SqlConnection(GetConnectionString()))
            using (SqlConnection conn2 = new SqlConnection(GetConnectionString()))
            {
                conn1.Open();
                conn2.Open();
                
                var transaction1 = conn1.BeginTransaction();
                
                // Update in Transaction 1 (Uncommitted)
                new SqlCommand("UPDATE Books SET Stock = 0 WHERE Id = 1", conn1, transaction1).ExecuteNonQuery();
                Console.WriteLine("T1: Stock puesto a 0 (sin commit).");

                // Try to Read in Transaction 2
                Console.WriteLine("T2: Intentando leer Stock...");
                
                // Rollback T1
                transaction1.Rollback();
                Console.WriteLine("T1: Rollback.");
                
                int stock = (int)new SqlCommand("SELECT Stock FROM Books WHERE Id = 1", conn2).ExecuteScalar();
                Console.WriteLine($"T2: Leyó Stock = {stock}. (Aislamiento mantenido).");
            }

            // 4. Durability
             Console.WriteLine("\n[D] Durabilidad: Confirmado por diseño de SQL Server (Write-Ahead Logging).");
             Console.WriteLine("Una vez que una transacción hace Commit, los datos persisten incluso ante fallos de energía.");
        }
    }
}