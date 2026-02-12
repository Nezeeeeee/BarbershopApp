using System;
using System.Data;
using System.Data.SQLite;

namespace BarbershopApp
{
    public class DatabaseHelper
    {
        private string connectionString = "Data Source=barbershop.db;Version=3;";

        public DatabaseHelper()
        {
            CreateDatabase();
        }

        // Создание базы данных и таблиц, если их нет
        private void CreateDatabase()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Полный SQL запрос для создания всех таблиц
                string createTablesScript = @"
                    CREATE TABLE IF NOT EXISTS Employees (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FullName TEXT NOT NULL,
                        Phone TEXT,
                        HireDate DATE,
                        IsActive BOOLEAN DEFAULT 1
                    );

                    CREATE TABLE IF NOT EXISTS Clients (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FullName TEXT NOT NULL,
                        Phone TEXT NOT NULL,
                        Email TEXT,
                        BirthDate DATE,
                        RegistrationDate DATE DEFAULT CURRENT_DATE,
                        Notes TEXT
                    );

                    CREATE TABLE IF NOT EXISTS Services (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Price DECIMAL(10,2) NOT NULL,
                        DurationMinutes INTEGER,
                        Description TEXT,
                        IsActive BOOLEAN DEFAULT 1
                    );

                    CREATE TABLE IF NOT EXISTS Appointments (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ClientId INTEGER NOT NULL,
                        EmployeeId INTEGER NOT NULL,
                        ServiceId INTEGER NOT NULL,
                        AppointmentDate DATE NOT NULL,
                        AppointmentTime TIME NOT NULL,
                        Status TEXT DEFAULT 'Запланирован',
                        Notes TEXT,
                        CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (ClientId) REFERENCES Clients(Id),
                        FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
                        FOREIGN KEY (ServiceId) REFERENCES Services(Id)
                    );

                    CREATE TABLE IF NOT EXISTS Payments (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        AppointmentId INTEGER NOT NULL,
                        Amount DECIMAL(10,2) NOT NULL,
                        PaymentDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        PaymentMethod TEXT,
                        FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id)
                    );
                ";

                using (var command = new SQLiteCommand(createTablesScript, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // Метод для выполнения запросов без возврата данных (INSERT, UPDATE, DELETE)
        public int ExecuteNonQuery(string query, SQLiteParameter[] parameters = null)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    return command.ExecuteNonQuery();
                }
            }
        }

        // Метод для получения данных (SELECT)
        public DataTable ExecuteQuery(string query, SQLiteParameter[] parameters = null)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }
    }
}