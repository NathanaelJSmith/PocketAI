using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

  class DataBaseManager
{
    //Creates file named pocketaid
    private string connectionString = "Data Source=pocketai.db";

    //Creates the database table if the don't already exist
    public void CreateTables()
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);

        connection.Open();

        string createExpenseTable = @"
        CREATE TABLE IF NOT EXISTS Expenses (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Name TEXT NOT NULL,
        Amount REAL NOT NULL,
        Category TEXT NOT NULL,
        Date TEXT NOT NULL);
        ";

        using SqliteCommand command = new SqliteCommand(createExpenseTable, connection);
        command.ExecuteNonQuery();
    }

    //Saves a new expense in the database 
    public void AddExpense(Expense expense)
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);

        connection.Open();

        string insertExpense = @"
        INSERT INTO Expenses (Name, Amount, Category, Date)
        VALUES (@Name, @Amount, @Category, @Date);
        ";

        using SqliteCommand command = new SqliteCommand(insertExpense, connection);

        //Add Values safely into the SQL command
        command.Parameters.AddWithValue("@Name", expense.Name);
        command.Parameters.AddWithValue("@Amount", expense.Amount);
        command.Parameters.AddWithValue("@Category", expense.Category);
        command.Parameters.AddWithValue("@Date", expense.Date.ToString("yyyy-MM-dd"));

        command.ExecuteNonQuery();
    }

    public List<Expense> GetAllExpenses()
    {
        List<Expense> expenses = new List<Expense>();

        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        string selectExpense = @"
        SELECT Id, Name, Amount, Category, Date
        FROM Expenses; 
        ";

        using SqliteCommand command = new SqliteCommand(selectExpense, connection);

        using SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            int id = reader.GetInt32(0);
            string name = reader.GetString(1);
            double amount = reader.GetDouble(2);
            string category = reader.GetString(3);
            DateTime date = DateTime.Parse(reader.GetString(4));

            Expense expense = new Expense(id, name, amount, category, date);

            expenses.Add(expense);
        }
        return expenses;
    }

}

