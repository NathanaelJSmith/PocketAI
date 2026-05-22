using Microsoft.Data.Sqlite;

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
        Category TEXT NOT NULL,
        Date TEXT NOT NULL);
        ";

        using SqliteCommand command = new SqliteCommand(createExpenseTable, connection);
        command.ExecuteNonQuery();
    }
}

