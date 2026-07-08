using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.Marshalling;

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
            Date TEXT NOT NULL
            );
        ";

        string createIncomeTable = @"
        CREATE TABLE IF NOT EXISTS Income (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Source TEXT NOT NULL,
            MonthlyAmount REAL NOT NULL
            );
        ";

        string createSavingsGoalTable = @"
        CREATE TABLE IF NOT EXISTS SavingsGoals (
            id INTEGER PRIMARY KEY, 
            Name TEXT NOT NULL,
            TargetAmount REAL NOT NULL,
            CurrentAmount REAL NOT NULL,
            DeadLine TEXT NOT NULL
            );
        ";

        string createBudgetLimitsTable = @"
        CREATE TABLE IF NOT EXISTS BudgetLimits (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Category TEXT NOT NULL,
            LimitAmount REAL NOT NULL
            );
        ";

        string createAccountBalanceTable = @"
        CREATE TABLE IF NOT EXISTS AccountBalance (
            Id INTEGER PRIMARY KEY, 
            CheckingBalance REAL NOT NULL,
            SavingsBalance REAL NOT NULL,
            CashBalance REAL NOT NULL
            );
        ";

        string createAIAdviceHistoryTable = @"
        CREATE TABLE IF NOT EXISTS AIAdviceHistory (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Prompt TEXT NOT NULL,
            AdviceText TEXT NOT NULL,
            DateCreated TEXT NOT NULL
        );";

        string createRecurringExpensesTable = @"
        CREATE TABLE IF NOT EXISTS RecurringExpenses (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Category TEXT NOT NULL,
            Amount REAL NOT NULL,
            DueDay INTEGER NOT NULL,
            IsActive INTEGER NOT NULL
            );";

        using SqliteCommand command = new SqliteCommand(createExpenseTable, connection);
        command.ExecuteNonQuery();

        using SqliteCommand incomeCommand = new SqliteCommand(createIncomeTable, connection);
        incomeCommand.ExecuteNonQuery();

        using SqliteCommand savingGoalCommand = new SqliteCommand(createSavingsGoalTable, connection);
        savingGoalCommand.ExecuteNonQuery();

        using SqliteCommand budgetLimitsCommand = new SqliteCommand(createBudgetLimitsTable, connection);
        budgetLimitsCommand.ExecuteNonQuery();

        using SqliteCommand accountBalanceCommand = new SqliteCommand(createAccountBalanceTable, connection);
        accountBalanceCommand.ExecuteNonQuery();

        using SqliteCommand aICommand = new SqliteCommand(createAIAdviceHistoryTable, connection); 
        aICommand.ExecuteNonQuery();

        using SqliteCommand recurringExpensesCommand = new SqliteCommand(createRecurringExpensesTable, connection);
        recurringExpensesCommand.ExecuteNonQuery();

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

    public void DeleteExpenseById(int id)
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);

        connection.Open();

        string deleteExpense = @"
        DELETE FROM Expenses
        WHERE Id = @Id;
        ";

        using SqliteCommand command = new SqliteCommand(deleteExpense, connection);
        command.Parameters.AddWithValue("@Id", id);

        command.ExecuteNonQuery();
    }

    //Updates an existing expense in the database
    public void UpdateExpense(Expense expense)
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);

        connection.Open();

        string updateExpense = @"
        UPDATE Expenses
        SET Name = @Name, Amount = @Amount, Category = @Category, Date = @Date
        WHERE Id = @Id;
        ";

        using SqliteCommand command = new SqliteCommand(updateExpense, connection);
        command.Parameters.AddWithValue("@Name", expense.Name);
        command.Parameters.AddWithValue("@Amount", expense.Amount);
        command.Parameters.AddWithValue("@Category", expense.Category);
        command.Parameters.AddWithValue("@Date", expense.Date.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@Id", expense.Id);

        command.ExecuteNonQuery();
    }

    //Saves or updates the user's income in the database.
    public void SaveIncome(Income income)
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        string saveIncome = @"
            INSERT OR REPLACE INTO Income (Id, Source, MonthlyAmount)
            VALUES (1, @Source, @MonthlyAmount);
        ";

        using SqliteCommand command = new SqliteCommand(saveIncome, connection);
        command.Parameters.AddWithValue("@Source", income.Source);
        command.Parameters.AddWithValue("@MonthlyAmount", income.MonthlyAmount);
        command.ExecuteNonQuery();

    }

    //Loads the user's income from the database
    public Income GetIncome()
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        string selectIncome = @"
            SELECT Source, MonthlyAmount
            FROM Income
            WHERE Id = 1;
        ";  

        using SqliteCommand command = new SqliteCommand(selectIncome, connection);

        using SqliteDataReader reader = command.ExecuteReader();

        if(reader.Read())
        {
            string source = reader.GetString(0);
            double monthlyAmount = reader.GetDouble(1);
            Income income = new Income(source, monthlyAmount);

            return new Income(source, monthlyAmount);

        }

        return null;
    }

    //Saves or updates the user's savings goal in the database.
    public void SaveSavingsGoal(SavingsGoal savingsGoal)
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        string saveSavingsGoal = @"
            INSERT OR REPLACE INTO SavingsGoals (id, Name, TargetAmount, CurrentAmount, DeadLine)
            VALUES (1, @Name, @TargetAmount, @CurrentAmount, @DeadLine);
        ";

        using SqliteCommand command = new SqliteCommand(saveSavingsGoal, connection);

            command.Parameters.AddWithValue("@Name", savingsGoal.Name);
            command.Parameters.AddWithValue("@TargetAmount", savingsGoal.TargetAmount);
            command.Parameters.AddWithValue("@CurrentAmount", savingsGoal.CurrentAmount);
            command.Parameters.AddWithValue("@DeadLine", savingsGoal.DeadLine.ToString("yyyy-MM-dd"));
            command.ExecuteNonQuery();
    }

    //Loads the user's savings goal from the database
    public SavingsGoal GetSavingsGoal()
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        string selectSavingGoal = @"
            SELECT Name, TargetAmount, CurrentAmount, DeadLine
            FROM SavingsGoals
            WHERE id = 1;
        ";

        using SqliteCommand command = new SqliteCommand(selectSavingGoal, connection);

        using SqliteDataReader reader = command.ExecuteReader();

        if (reader.Read())
        {
            string name = reader.GetString(0);
            double targetAmount = reader.GetDouble(1);
            double currentAmount = reader.GetDouble(2);
            DateTime deadline = DateTime.Parse(reader.GetString(3));

            return new SavingsGoal(name, targetAmount, currentAmount, deadline);
            
        }
        return null;
    }

    //Saves or updates one budget limit in the database
    public void SaveBudgetLimit(BudgetLimit budgetLimit)
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        string saveBudgetLimit = @"
            INSERT INTO BudgetLimits (Category, LimitAmount)
            VALUES (@Category, @LimitAmount);
        ";

        using SqliteCommand command = new SqliteCommand(saveBudgetLimit, connection);

        command.Parameters.AddWithValue("@Category", budgetLimit.Category);
        command.Parameters.AddWithValue("@LimitAmount", budgetLimit.LimitAmount);

        command.ExecuteNonQuery();
    }

    //Loads all budget limits from the database 
    public List<BudgetLimit> GetBudgetLimits()
    {
        List<BudgetLimit> budgetLimits = new List<BudgetLimit>();

        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        string selectBudgetLimits = @"
            SELECT Category, LimitAmount
            FROM BudgetLimits;
        ";

        using SqliteCommand SqliteCommand = new SqliteCommand(selectBudgetLimits, connection);

        using SqliteDataReader reader = SqliteCommand.ExecuteReader();
        
        while (reader.Read())
        {
            string category = reader.GetString(0);
            double limitAmount = reader.GetDouble(1);

            //Builds a budget limit object from the database row
            BudgetLimit budgetLimit = new BudgetLimit(category, limitAmount);
           
            budgetLimits.Add(budgetLimit);
        }

        return budgetLimits;
    }

    //Delete a budget limit from the database by category name
    public void DeleteBudgetLimitsByCategory(string category)
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        string deleteBudgetLimit = @"
            DELETE FROM BudgetLimits
            WHERE Category = @Category;
        ";

        using SqliteCommand command = new SqliteCommand(deleteBudgetLimit, connection);

        //Adds category name safely into the SQL command
        command.Parameters.AddWithValue("@Category", category);
        command.ExecuteNonQuery();

    }

    public void SaveAccountBalance(AccountBalance accountBalance)
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        string saveAccountBalance = @"
            INSERT OR REPLACE INTO AccountBalance (Id, CheckingBalance, SavingsBalance, CashBalance)
            VALUES (1, @CheckingBalance, @SavingsBalance, @CashBalance);
        ";

        using SqliteCommand command = new SqliteCommand(saveAccountBalance, connection);

        //Adds account balance values safely into the SQL command
        command.Parameters.AddWithValue("@CheckingBalance", accountBalance.CheckingBalance);
        command.Parameters.AddWithValue("@SavingsBalance", accountBalance.SavingsBalance);
        command.Parameters.AddWithValue("@CashBalance", accountBalance.CashBalance);

        command.ExecuteNonQuery();

    }
    public AccountBalance GetAccountBalance()
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);

        connection.Open();

        string selectAccountBalance = @"
            SELECT CheckingBalance, SavingsBalance, CashBalance
            FROM AccountBalance
            WHERE Id = 1;
        ";

        using SqliteCommand command = new SqliteCommand(selectAccountBalance, connection);

        using SqliteDataReader reader = command.ExecuteReader();

        if (reader.Read())
        {
            double checkingBalance = reader.GetDouble(0);
            double savingsBalance = reader.GetDouble(1);
            double cashBalance = reader.GetDouble(2);

            //Builds an account balance object from the database row
            return new AccountBalance(checkingBalance, savingsBalance, cashBalance);
        }

        return null;
    }

    public void SaveAIAdvice(string prompt, string adviceText)
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        string insertAIAdvice = @"
        INSERT INTO AIAdviceHistory (Prompt, AdviceText, DateCreated)
        VALUES (@Prompt, @AdviceText, @DateCreated);";

        using SqliteCommand command = new SqliteCommand(@insertAIAdvice, connection);
        command.Parameters.AddWithValue("@Prompt", prompt);
        command.Parameters.AddWithValue("@AdviceText", adviceText);
        command.Parameters.AddWithValue(@"DateCreated", DateTime.Now.ToString());
            command.ExecuteNonQuery();

        
    }

    public List<AIAdvice> GetAIAdviceHistory()
    {
        List<AIAdvice> adviceHistory = new List<AIAdvice>();

        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        string selectAIAdvice = @"
        SELECT Id, Prompt, AdviceText, DateCreated
        FROM AIAdviceHistory
        ORDER BY Id DESC;";

        using SqliteCommand command = new SqliteCommand(selectAIAdvice, connection);

        using SqliteDataReader reader = command.ExecuteReader();

        while(reader.Read())
        {
            int id = reader.GetInt32(0);
            string prompt = reader.GetString(1);
            string adviceText = reader.GetString(2);
            DateTime dateCreated = DateTime.Parse(reader.GetString(3));

            AIAdvice advice = new AIAdvice(id, prompt, adviceText, dateCreated);

            adviceHistory.Add(advice);
        }

        return adviceHistory;
    }

    public AIAdvice? GetAIAdviceById(int id)
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        string selectAIAdviceById = @"
        SELECT Id, Prompt, AdviceText, DateCreated
        FROM AIAdviceHistory
        WHERE Id = @Id;";

        using SqliteCommand command = new SqliteCommand(selectAIAdviceById, connection);
        command.Parameters.AddWithValue("@Id", id);

        using SqliteDataReader reader = command.ExecuteReader();

        if (reader.Read())
        {
            int adviceId = reader.GetInt32(0);
            string prompt = reader.GetString(1);
            string adviceText = reader.GetString(2);
            DateTime dateCreated = DateTime.Parse(reader.GetString(3));

            return new AIAdvice(adviceId, prompt, adviceText, dateCreated);
        }
        return null;
    }

    public bool DeleteAIAdviceById(int id)
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        string deleteAIAdvice = @"
        DELETE FROM AIAdviceHistory
        WHERE Id = @Id;";

        using SqliteCommand command = new SqliteCommand(deleteAIAdvice, connection);

        command.Parameters.AddWithValue("@Id", id);

        int rowsAffected = command.ExecuteNonQuery();

        return rowsAffected > 0;
    }

    //Searches saved AI advice records by keyword 
    public List<AIAdvice> SearchAIAdvice(string keyword)
    {
        List<AIAdvice> results = new List<AIAdvice>();

        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        string searchAIAdvice = @"
        Select Id, Prompt, AdviceText, DateCreated
        FROM AIAdviceHistory
        WHERE AdviceText LIKE @Keyword
        OR PROMPT LIKE @Keyword
        ORDER BY Id DESC;";

        using SqliteCommand command = new SqliteCommand(searchAIAdvice, connection);

        command.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

        using SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            int id = reader.GetInt32(0);
            string prompt = reader.GetString(1);
            string adviceText = reader.GetString(2);
            DateTime dateCreated = DateTime.Parse(reader.GetString(3));
            AIAdvice advice = new AIAdvice(id, prompt, adviceText, dateCreated);

            results.Add(advice);
        }

        return results;
    }

    //Add a recurring expense to the database
    public void AddRecurringExpense(RecurringExpenses expense)
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();
        
        string insertRecurringExpense = @"
        INSERT INTO RecurringExpenses (Name, Category, Amount, DueDay, IsActive)
        VALUES (@Name, @Category, @Amount, @DueDay, @IsActive);
        ";
        using SqliteCommand command = new SqliteCommand(insertRecurringExpense, connection);

        command.Parameters.AddWithValue("@Name", expense.Name);
        command.Parameters.AddWithValue("@Category", expense.Category);
        command.Parameters.AddWithValue("@Amount", expense.Amount);
        command.Parameters.AddWithValue("@DueDay", expense.DueDay);
        command.Parameters.AddWithValue("@IsActive", expense.IsActive ? 1 : 0);

        command.ExecuteNonQuery();  
    }

    //gets all recurring expenses from the database
    public List<RecurringExpenses> GetRecuringExpenses()
    {
        List<RecurringExpenses> expenses = new List<RecurringExpenses>();

        using SqliteConnection connection = new SqliteConnection(connectionString);

        connection.Open();

        string query = @"
        SELECT Id, Name, Category, Amount, DueDay, IsActive
        FROM RecurringExpenses;
        WHERE IsActive = 1;
        ";

        using SqliteCommand command = new SqliteCommand(query, connection);

        using SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            expenses.Add(new RecurringExpenses(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetDouble(3),
                reader.GetInt32(4),
                reader.GetInt32(5) == 1
            ));
        }

        return expenses;
    }

}

