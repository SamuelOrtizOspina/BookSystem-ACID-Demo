-- Create Database if not exists
IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'BookStoreDB')
BEGIN
    CREATE DATABASE BookStoreDB;
END
GO

USE BookStoreDB;
GO

-- Create Authors Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Authors')
BEGIN
    CREATE TABLE Authors (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name VARCHAR(100) NOT NULL,
        Bio TEXT
    );
END
GO

-- Create Books Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Books')
BEGIN
    CREATE TABLE Books (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Title VARCHAR(200) NOT NULL,
        ISBN VARCHAR(20) UNIQUE,
        Price DECIMAL(10, 2) NOT NULL,
        Stock INT NOT NULL DEFAULT 0,
        AuthorId INT NOT NULL,
        CONSTRAINT FK_Books_Authors FOREIGN KEY (AuthorId) REFERENCES Authors(Id)
    );
END
GO

-- Seed Initial Data
IF NOT EXISTS (SELECT * FROM Authors WHERE Name = 'Gabriel Garcia Marquez')
BEGIN
    INSERT INTO Authors (Name, Bio) VALUES ('Gabriel Garcia Marquez', 'Colombian novelist.');
END

IF NOT EXISTS (SELECT * FROM Authors WHERE Name = 'J.K. Rowling')
BEGIN
    INSERT INTO Authors (Name, Bio) VALUES ('J.K. Rowling', 'British author, best known for Harry Potter.');
END

IF NOT EXISTS (SELECT * FROM Books WHERE ISBN = '978-0307474728')
BEGIN
    INSERT INTO Books (Title, ISBN, Price, Stock, AuthorId) VALUES
    ('Cien Anos de Soledad', '978-0307474728', 15.99, 100, 1);
END

IF NOT EXISTS (SELECT * FROM Books WHERE ISBN = '978-0590353427')
BEGIN
    INSERT INTO Books (Title, ISBN, Price, Stock, AuthorId) VALUES
    ('Harry Potter and the Sorcerers Stone', '978-0590353427', 12.99, 50, 2);
END
GO