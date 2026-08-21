using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.Marshalling;

public class DataBaseManager
{
    //Creates file named pocketaid
    private readonly string connectionString = "Data Source=pocketai.db";

    // Used by the original console application
    public DataBaseManager()
    {   
    connectionString = "Data Source=pocketai.db";
    }

    // Allows another application to choose where the database is stored
    public DataBaseManager(string databasePath)
    {
    connectionString = $"Data Source={databasePath}";
    }
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
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            TargetAmount REAL NOT NULL,
            CurrentAmount REAL NOT NULL,
            DeadLine TEXT NOT NULL,
            IsPrimary INTEGER NOT NULL DEFAULT 0
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

        // Makes older PocketAI databases compatible
        // with multiple savings goals.
        EnsureSavingsGoalColumns(connection);

        using SqliteCommand budgetLimitsCommand = new SqliteCommand(createBudgetLimitsTable, connection);
        budgetLimitsCommand.ExecuteNonQuery();

        using SqliteCommand accountBalanceCommand = new SqliteCommand(createAccountBalanceTable, connection);
        accountBalanceCommand.ExecuteNonQuery();

        using SqliteCommand aICommand = new SqliteCommand(createAIAdviceHistoryTable, connection); 
        aICommand.ExecuteNonQuery();

        using SqliteCommand recurringExpensesCommand = new SqliteCommand(createRecurringExpensesTable, connection);
        recurringExpensesCommand.ExecuteNonQuery();

    }


    private void EnsureSavingsGoalColumns(
        SqliteConnection connection)
    {
        bool hasIsPrimaryColumn = false;

        // Check the existing SavingsGoals table
        using (SqliteCommand command =
            new SqliteCommand(
                "PRAGMA table_info(SavingsGoals);",
                connection))
        {
            using SqliteDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                string columnName =
                    reader.GetString(1);

                if (columnName.Equals(
                    "IsPrimary",
                    StringComparison.OrdinalIgnoreCase))
                {
                    hasIsPrimaryColumn = true;
                    break;
                }
            }
        }


        // Older databases will not have this column
        if (!hasIsPrimaryColumn)
        {
            using SqliteCommand alterCommand =
                new SqliteCommand(
                    @"ALTER TABLE SavingsGoals
                    ADD COLUMN IsPrimary
                    INTEGER NOT NULL DEFAULT 0;",
                    connection);

            alterCommand.ExecuteNonQuery();
        }


        // If an old savings goal already exists,
        // make the first one the primary goal.
        using SqliteCommand primaryCommand =
            new SqliteCommand(
                @"
                UPDATE SavingsGoals
                SET IsPrimary = 1
                WHERE Id = (
                    SELECT Id
                    FROM SavingsGoals
                    ORDER BY Id
                    LIMIT 1
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM SavingsGoals
                    WHERE IsPrimary = 1
                );
                ",
                connection);

        primaryCommand.ExecuteNonQuery();
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
   // ==========================================
    // ADD A NEW SAVINGS GOAL
    // ==========================================

    public void AddSavingsGoal(
        SavingsGoal savingsGoal)
    {
        using SqliteConnection connection =
            new SqliteConnection(connectionString);

        connection.Open();


        // First goal automatically becomes primary
        string countGoals = @"
            SELECT COUNT(*)
            FROM SavingsGoals;
        ";

        using SqliteCommand countCommand =
            new SqliteCommand(
                countGoals,
                connection);

        long goalCount =
            (long)(countCommand.ExecuteScalar() ?? 0L);


        bool shouldBePrimary =
            savingsGoal.IsPrimary ||
            goalCount == 0;


        string insertGoal = @"
            INSERT INTO SavingsGoals
            (
                Name,
                TargetAmount,
                CurrentAmount,
                DeadLine,
                IsPrimary
            )
            VALUES
            (
                @Name,
                @TargetAmount,
                @CurrentAmount,
                @DeadLine,
                @IsPrimary
            );
        ";


        using SqliteCommand command =
            new SqliteCommand(
                insertGoal,
                connection);

        command.Parameters.AddWithValue(
            "@Name",
            savingsGoal.Name);

        command.Parameters.AddWithValue(
            "@TargetAmount",
            savingsGoal.TargetAmount);

        command.Parameters.AddWithValue(
            "@CurrentAmount",
            savingsGoal.CurrentAmount);

        command.Parameters.AddWithValue(
            "@DeadLine",
            savingsGoal.DeadLine
                .ToString("yyyy-MM-dd"));

        command.Parameters.AddWithValue(
            "@IsPrimary",
            shouldBePrimary ? 1 : 0);


        command.ExecuteNonQuery();
    }



    // ==========================================
    // LOAD ALL SAVINGS GOALS
    // ==========================================

    public List<SavingsGoal> GetSavingsGoals()
    {
        List<SavingsGoal> savingsGoals =
            new List<SavingsGoal>();


        using SqliteConnection connection =
            new SqliteConnection(connectionString);

        connection.Open();


        string selectGoals = @"
            SELECT
                Id,
                Name,
                TargetAmount,
                CurrentAmount,
                DeadLine,
                IsPrimary
            FROM SavingsGoals
            ORDER BY IsPrimary DESC, Id ASC;
        ";


        using SqliteCommand command =
            new SqliteCommand(
                selectGoals,
                connection);

        using SqliteDataReader reader =
            command.ExecuteReader();


        while (reader.Read())
        {
            int id =
                reader.GetInt32(0);

            string name =
                reader.GetString(1);

            double targetAmount =
                reader.GetDouble(2);

            double currentAmount =
                reader.GetDouble(3);

            DateTime deadLine =
                DateTime.Parse(
                    reader.GetString(4));

            bool isPrimary =
                reader.GetInt32(5) == 1;


            SavingsGoal savingsGoal =
                new SavingsGoal(
                    id,
                    name,
                    targetAmount,
                    currentAmount,
                    deadLine,
                    isPrimary);


            savingsGoals.Add(
                savingsGoal);
        }


        return savingsGoals;
    }



    // ==========================================
    // UPDATE EXISTING SAVINGS GOAL
    // ==========================================

    public void UpdateSavingsGoal(
        SavingsGoal savingsGoal)
    {
        using SqliteConnection connection =
            new SqliteConnection(connectionString);

        connection.Open();


        string updateGoal = @"
            UPDATE SavingsGoals
            SET
                Name = @Name,
                TargetAmount = @TargetAmount,
                CurrentAmount = @CurrentAmount,
                DeadLine = @DeadLine
            WHERE Id = @Id;
        ";


        using SqliteCommand command =
            new SqliteCommand(
                updateGoal,
                connection);

        command.Parameters.AddWithValue(
            "@Name",
            savingsGoal.Name);

        command.Parameters.AddWithValue(
            "@TargetAmount",
            savingsGoal.TargetAmount);

        command.Parameters.AddWithValue(
            "@CurrentAmount",
            savingsGoal.CurrentAmount);

        command.Parameters.AddWithValue(
            "@DeadLine",
            savingsGoal.DeadLine
                .ToString("yyyy-MM-dd"));

        command.Parameters.AddWithValue(
            "@Id",
            savingsGoal.Id);


        command.ExecuteNonQuery();
    }



    // ==========================================
    // DELETE SAVINGS GOAL
    // ==========================================

    public void DeleteSavingsGoalById(
        int id)
    {
        using SqliteConnection connection =
            new SqliteConnection(connectionString);

        connection.Open();


        // Check whether the deleted goal was primary
        string primaryCheck = @"
            SELECT IsPrimary
            FROM SavingsGoals
            WHERE Id = @Id;
        ";


        bool wasPrimary = false;


        using (SqliteCommand checkCommand =
            new SqliteCommand(
                primaryCheck,
                connection))
        {
            checkCommand.Parameters.AddWithValue(
                "@Id",
                id);

            object? result =
                checkCommand.ExecuteScalar();

            if (result != null)
            {
                wasPrimary =
                    Convert.ToInt32(result) == 1;
            }
        }


        string deleteGoal = @"
            DELETE FROM SavingsGoals
            WHERE Id = @Id;
        ";


        using (SqliteCommand command =
            new SqliteCommand(
                deleteGoal,
                connection))
        {
            command.Parameters.AddWithValue(
                "@Id",
                id);

            command.ExecuteNonQuery();
        }


        // If primary was deleted,
        // promote the next goal automatically
        if (wasPrimary)
        {
            string promoteNextGoal = @"
                UPDATE SavingsGoals
                SET IsPrimary = 1
                WHERE Id = (
                    SELECT Id
                    FROM SavingsGoals
                    ORDER BY Id
                    LIMIT 1
                );
            ";


            using SqliteCommand promoteCommand =
                new SqliteCommand(
                    promoteNextGoal,
                    connection);

            promoteCommand.ExecuteNonQuery();
        }
    }



    // ==========================================
    // SET PRIMARY SAVINGS GOAL
    // ==========================================

    public void SetPrimarySavingsGoal(
        int id)
    {
        using SqliteConnection connection =
            new SqliteConnection(connectionString);

        connection.Open();


        using SqliteTransaction transaction =
            connection.BeginTransaction();


        // Remove primary status from every goal
        using (SqliteCommand clearCommand =
            new SqliteCommand(
                @"UPDATE SavingsGoals
                SET IsPrimary = 0;",
                connection,
                transaction))
        {
            clearCommand.ExecuteNonQuery();
        }


        // Set the selected goal as primary
        using (SqliteCommand primaryCommand =
            new SqliteCommand(
                @"UPDATE SavingsGoals
                SET IsPrimary = 1
                WHERE Id = @Id;",
                connection,
                transaction))
        {
            primaryCommand.Parameters.AddWithValue(
                "@Id",
                id);

            primaryCommand.ExecuteNonQuery();
        }


        transaction.Commit();
    }



    // ==========================================
    // GET PRIMARY SAVINGS GOAL
    // ==========================================

    public SavingsGoal? GetPrimarySavingsGoal()
    {
        List<SavingsGoal> goals =
            GetSavingsGoals();


        return goals.FirstOrDefault(
                goal => goal.IsPrimary)
            ??
            goals.FirstOrDefault();
    }



    // ==========================================
    // BACKWARDS COMPATIBILITY
    // ==========================================

    // Existing Home/Savings code currently calls this.
    // For now it returns the primary savings goal.
    public SavingsGoal? GetSavingsGoal()
    {
        return GetPrimarySavingsGoal();
    }


    // Existing single-goal Savings page still calls
    // SaveSavingsGoal. Keep it functional until we
    // replace the Savings UI in the next step.
    public void SaveSavingsGoal(
        SavingsGoal savingsGoal)
    {
        SavingsGoal? existingPrimary =
            GetPrimarySavingsGoal();


        if (existingPrimary == null)
        {
            savingsGoal.IsPrimary = true;

            AddSavingsGoal(
                savingsGoal);

            return;
        }


        savingsGoal.Id =
            existingPrimary.Id;

        savingsGoal.IsPrimary =
            true;


        UpdateSavingsGoal(
            savingsGoal);
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

