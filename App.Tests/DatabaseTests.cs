using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Xunit;

namespace App.Tests
{
    public class DatabaseTests
    {
        private string GetConnectionString(string dbName = "BookStoreDB")
        {
            // For local tests against Docker, we use localhost
            return $"Server=localhost,1433;Database={dbName};User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";
        }

        [Fact]
        public void TestConnection_ShouldSucceed()
        {
            using (var conn = new SqlConnection(GetConnectionString("master")))
            {
                conn.Open();
                Assert.Equal(ConnectionState.Open, conn.State);
            }
        }

        [Fact]
        public void CreateDatabase_ShouldSucceed()
        {
            using (var conn = new SqlConnection(GetConnectionString("master")))
            {
                conn.Open();
                var cmd = new SqlCommand("IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'TestDB') CREATE DATABASE TestDB", conn);
                cmd.ExecuteNonQuery();
            }

            using (var conn = new SqlConnection(GetConnectionString("TestDB")))
            {
                conn.Open();
                Assert.Equal(ConnectionState.Open, conn.State);
            }
        }

        [Fact]
        public void CRUD_Operations_ShouldSucceed()
        {
            // Use the main DB for integration test
            string connStr = GetConnectionString();
            
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                
                // 1. Insert
                var insertCmd = new SqlCommand("INSERT INTO Authors (Name, Bio) VALUES ('Test Author', 'Bio content')", conn);
                insertCmd.ExecuteNonQuery();
                
                // 2. Read
                var selectCmd = new SqlCommand("SELECT TOP 1 Id FROM Authors WHERE Name = 'Test Author' ORDER BY Id DESC", conn);
                int authorId = Convert.ToInt32(selectCmd.ExecuteScalar());
                Assert.True(authorId > 0);

                // 3. Update
                var updateCmd = new SqlCommand($"UPDATE Authors SET Name = 'Updated Author' WHERE Id = {authorId}", conn);
                updateCmd.ExecuteNonQuery();

                // 4. Verify Update
                var verifyCmd = new SqlCommand($"SELECT Name FROM Authors WHERE Id = {authorId}", conn);
                string name = (string)verifyCmd.ExecuteScalar();
                Assert.Equal("Updated Author", name);

                // 5. Delete
                // First delete books associated (if any, though here shouldn't be)
                new SqlCommand($"DELETE FROM Books WHERE AuthorId = {authorId}", conn).ExecuteNonQuery();
                var deleteCmd = new SqlCommand($"DELETE FROM Authors WHERE Id = {authorId}", conn);
                deleteCmd.ExecuteNonQuery();
            }
        }

        [Fact]
        public void ACID_Atomicity_Rollback_ShouldWork()
        {
            string connStr = GetConnectionString();
            decimal originalPrice = 0;
            int bookId = 1;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                // Get original price
                originalPrice = (decimal)new SqlCommand($"SELECT Price FROM Books WHERE Id = {bookId}", conn).ExecuteScalar();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var cmd = new SqlCommand($"UPDATE Books SET Price = 99.99 WHERE Id = {bookId}", conn, transaction);
                        cmd.ExecuteNonQuery();
                        
                        // Force failure
                        throw new Exception("Simulated failure");
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }

                // Verify price hasn't changed
                decimal currentPrice = (decimal)new SqlCommand($"SELECT Price FROM Books WHERE Id = {bookId}", conn).ExecuteScalar();
                Assert.Equal(originalPrice, currentPrice);
            }
        }
    }
}
