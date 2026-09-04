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
            Date TEXT NOT NULL,
            PaidFromAccount TEXT NOT NULL
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
            IsPrimary INTEGER NOT NULL DEFAULT 0,
            Priorityrank INTEGER NOT NULL DEFAULT 0,
            IsEssential INTEGER NOT NULL DEFAULT 0,
            CustomAllocationPercentage REAL NULL,
            IsCompleted INTEGER NOT NULL DEFAULT 0,
            DateCreated TEXT NOT NULL,
            DateCompleted TEXT NULL
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

        string createRecurringBillPaymentTable = @"
        CREATE TABLE IF NOT EXISTS RecurringBillPayment
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            RecurringExpenseId INTEGER NOT NULL,
            MonthKey TEXT NOT NULL,
            IsPaid INTEGER NOT NULL DEFAULT 0,
            DatePaid TEXT NULL,

            UNIQUE
            (
                RecurringExpenseId,
                MonthKey
            )
        );
        ";

        string createAcceptedExtraSavingsTable = @"
        CREATE TABLE IF NOT EXISTS AcceptedExtraSavings (
            MonthKey TEXT PRIMARY KEY,
            Amount REAL NOT NULL
            );";

        using SqliteCommand command = new SqliteCommand(createExpenseTable, connection);
        command.ExecuteNonQuery();
        EnsureExpenseColumns(connection);

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
        
        using SqliteCommand recurringBillPaymentsCommand = new SqliteCommand(createRecurringBillPaymentTable, connection);
        recurringBillPaymentsCommand.ExecuteNonQuery();

        using SqliteCommand acceptedExtraSavingsCommand = new SqliteCommand(createAcceptedExtraSavingsTable, connection);
        acceptedExtraSavingsCommand.ExecuteNonQuery();

    }


    // ==========================================
    // MAKE OLD EXPENSE DATABASES COMPATIBLE
    // ==========================================

    private void EnsureExpenseColumns(
        SqliteConnection connection)
    {
        HashSet<string> existingColumns =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);


        // ======================================
        // FIND EXISTING EXPENSE COLUMNS
        // ======================================

        using (SqliteCommand command =
            new SqliteCommand(
                "PRAGMA table_info(Expenses);",
                connection))
        {
            using SqliteDataReader reader =
                command.ExecuteReader();


            while (reader.Read())
            {
                string columnName =
                    reader.GetString(1);


                existingColumns.Add(
                    columnName);
            }
        }


        // ======================================
        // PAID FROM ACCOUNT
        // ======================================

        if (!existingColumns.Contains(
                "PaidFromAccount"))
        {
            using SqliteCommand command =
                new SqliteCommand(
                    @"
                    ALTER TABLE Expenses
                    ADD COLUMN PaidFromAccount
                    TEXT NULL;
                    ",
                    connection);


            command.ExecuteNonQuery();
        }
    }
    // ==========================================
    // MAKE OLD SAVINGS DATABASES COMPATIBLE
    // ==========================================

    private void EnsureSavingsGoalColumns(
        SqliteConnection connection)
    {
        // ======================================
        // FIND EXISTING COLUMNS
        // ======================================

        HashSet<string> existingColumns =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);


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


                existingColumns.Add(
                    columnName);
            }
        }



        // ======================================
        // IS PRIMARY
        // ======================================

        if (!existingColumns.Contains(
                "IsPrimary"))
        {
            using SqliteCommand command =
                new SqliteCommand(
                    @"
                    ALTER TABLE SavingsGoals
                    ADD COLUMN IsPrimary
                    INTEGER NOT NULL DEFAULT 0;
                    ",
                    connection);


            command.ExecuteNonQuery();
        }



        // ======================================
        // PRIORITY RANK
        // ======================================

        if (!existingColumns.Contains(
                "PriorityRank"))
        {
            using SqliteCommand command =
                new SqliteCommand(
                    @"
                    ALTER TABLE SavingsGoals
                    ADD COLUMN PriorityRank
                    INTEGER NOT NULL DEFAULT 0;
                    ",
                    connection);


            command.ExecuteNonQuery();
        }



        // ======================================
        // ESSENTIAL GOAL
        // ======================================

        if (!existingColumns.Contains(
                "IsEssential"))
        {
            using SqliteCommand command =
                new SqliteCommand(
                    @"
                    ALTER TABLE SavingsGoals
                    ADD COLUMN IsEssential
                    INTEGER NOT NULL DEFAULT 0;
                    ",
                    connection);


            command.ExecuteNonQuery();
        }



        // ======================================
        // CUSTOM ALLOCATION
        // ======================================

        if (!existingColumns.Contains(
                "CustomAllocationPercentage"))
        {
            using SqliteCommand command =
                new SqliteCommand(
                    @"
                    ALTER TABLE SavingsGoals
                    ADD COLUMN CustomAllocationPercentage
                    REAL NULL;
                    ",
                    connection);


            command.ExecuteNonQuery();
        }

        // ======================================
        // COMPLETED GOAL STATUS
        // ======================================

        if (!existingColumns.Contains(
                "IsCompleted"))
        {
            using SqliteCommand command =
                new SqliteCommand(
                    @"
                    ALTER TABLE SavingsGoals
                    ADD COLUMN IsCompleted
                    INTEGER NOT NULL DEFAULT 0;
                    ",
                    connection);


            command.ExecuteNonQuery();
        }



        // ======================================
        // DATE CREATED
        // ======================================

        if (!existingColumns.Contains(
                "DateCreated"))
        {
            using SqliteCommand command =
                new SqliteCommand(
                    @"
                    ALTER TABLE SavingsGoals
                    ADD COLUMN DateCreated
                    TEXT NULL;
                    ",
                    connection);


            command.ExecuteNonQuery();
        }



        // ======================================
        // DATE COMPLETED
        // ======================================

        if (!existingColumns.Contains(
                "DateCompleted"))
        {
            using SqliteCommand command =
                new SqliteCommand(
                    @"
                    ALTER TABLE SavingsGoals
                    ADD COLUMN DateCompleted
                    TEXT NULL;
                    ",
                    connection);


            command.ExecuteNonQuery();
        }


        // ======================================
        // MAKE SURE A PRIMARY GOAL EXISTS
        // ======================================

        // This keeps your current Home-page
        // savings behavior working.
        using (SqliteCommand primaryCommand =
            new SqliteCommand(
                @"
                UPDATE SavingsGoals

                SET IsPrimary = 1

                WHERE Id =
                (
                    SELECT Id
                    FROM SavingsGoals
                    ORDER BY Id
                    LIMIT 1
                )

                AND NOT EXISTS
                (
                    SELECT 1
                    FROM SavingsGoals
                    WHERE IsPrimary = 1
                );
                ",
                connection))
        {
            primaryCommand.ExecuteNonQuery();
        }



        // ======================================
        // ASSIGN PRIORITIES TO OLD GOALS
        // ======================================

        // Existing users already have goals,
        // but those goals will initially have
        // PriorityRank = 0.
        //
        // We assign them safe starting ranks.
        //
        // The existing Primary goal receives
        // Priority 1 ONLY as an initial migration
        // default.
        //
        // After migration, IsPrimary and
        // PriorityRank remain independent.

        int highestExistingPriority =
            0;


        using (SqliteCommand command =
            new SqliteCommand(
                @"
                SELECT COALESCE(
                    MAX(PriorityRank),
                    0
                )

                FROM SavingsGoals

                WHERE PriorityRank > 0;
                ",
                connection))
        {
            object? result =
                command.ExecuteScalar();


            highestExistingPriority =
                Convert.ToInt32(
                    result ?? 0);
        }



        int nextPriorityRank =
            highestExistingPriority + 1;



        List<int> unrankedGoalIds =
            new List<int>();


        using (SqliteCommand command =
            new SqliteCommand(
                @"
                SELECT Id

                FROM SavingsGoals

                WHERE PriorityRank <= 0

                ORDER BY
                    IsPrimary DESC,
                    Id ASC;
                ",
                connection))
        {
            using SqliteDataReader reader =
                command.ExecuteReader();


            while (reader.Read())
            {
                unrankedGoalIds.Add(
                    reader.GetInt32(0));
            }
        }



        // Assign sequential ranks:
        //
        // Priority 1
        // Priority 2
        // Priority 3
        // etc.

        foreach (int goalId
                in unrankedGoalIds)
        {
            using SqliteCommand command =
                new SqliteCommand(
                    @"
                    UPDATE SavingsGoals

                    SET PriorityRank =
                        @PriorityRank

                    WHERE Id =
                        @Id;
                    ",
                    connection);


            command.Parameters.AddWithValue(
                "@PriorityRank",
                nextPriorityRank);


            command.Parameters.AddWithValue(
                "@Id",
                goalId);


            command.ExecuteNonQuery();


            nextPriorityRank++;
        }
    }

    // ==========================================
    // ADD EXPENSE
    // ==========================================

    public void AddExpense(
        Expense expense)
    {
        using SqliteConnection connection =
            new SqliteConnection(
                connectionString);


        connection.Open();


        using SqliteTransaction transaction =
            connection.BeginTransaction();


        // ======================================
        // SAVE EXPENSE
        // ======================================

        string insertExpense = @"
            INSERT INTO Expenses
            (
                Name,
                Amount,
                Category,
                Date,
                PaidFromAccount
            )

            VALUES
            (
                @Name,
                @Amount,
                @Category,
                @Date,
                @PaidFromAccount
            );
        ";


        using (SqliteCommand command =
            new SqliteCommand(
                insertExpense,
                connection,
                transaction))
        {
            command.Parameters.AddWithValue(
                "@Name",
                expense.Name);


            command.Parameters.AddWithValue(
                "@Amount",
                expense.Amount);


            command.Parameters.AddWithValue(
                "@Category",
                expense.Category);


            command.Parameters.AddWithValue(
                "@Date",
                expense.Date.ToString(
                    "yyyy-MM-dd"));


            command.Parameters.AddWithValue(
                "@PaidFromAccount",
                expense.PaidFromAccount is null
                    ? DBNull.Value
                    : expense.PaidFromAccount);


            command.ExecuteNonQuery();
        }


        // ======================================
        // REDUCE SELECTED ACCOUNT
        // ======================================

        AdjustAccountBalance(
            connection,
            transaction,
            expense.PaidFromAccount,
            -expense.Amount);


        transaction.Commit();
    }

    // ==========================================
    // GET ALL EXPENSES
    // ==========================================

    public List<Expense> GetAllExpenses()
    {
        List<Expense> expenses =
            new List<Expense>();


        using SqliteConnection connection =
            new SqliteConnection(
                connectionString);


        connection.Open();


        string selectExpense = @"
            SELECT
                Id,
                Name,
                Amount,
                Category,
                Date,
                PaidFromAccount

            FROM Expenses;
        ";


        using SqliteCommand command =
            new SqliteCommand(
                selectExpense,
                connection);


        using SqliteDataReader reader =
            command.ExecuteReader();


        while (reader.Read())
        {
            int id =
                reader.GetInt32(0);


            string name =
                reader.GetString(1);


            double amount =
                reader.GetDouble(2);


            string category =
                reader.GetString(3);


            DateTime date =
                DateTime.Parse(
                    reader.GetString(4));


            string? paidFromAccount =
                reader.IsDBNull(5)
                    ? null
                    : reader.GetString(5);


            Expense expense =
                new Expense(
                    id,
                    name,
                    amount,
                    category,
                    date,
                    paidFromAccount);


            expenses.Add(
                expense);
        }


        return expenses;
    }

    // ==========================================
    // DELETE EXPENSE
    // ==========================================

    public void DeleteExpenseById(
        int id)
    {
        using SqliteConnection connection =
            new SqliteConnection(
                connectionString);


        connection.Open();


        using SqliteTransaction transaction =
            connection.BeginTransaction();


        double expenseAmount =
            0;


        string? paidFromAccount =
            null;


        bool expenseFound =
            false;


        // ======================================
        // LOAD EXPENSE BEFORE DELETING
        // ======================================

        using (SqliteCommand findCommand =
            new SqliteCommand(
                @"
                SELECT
                    Amount,
                    PaidFromAccount

                FROM Expenses

                WHERE Id = @Id;
                ",
                connection,
                transaction))
        {
            findCommand.Parameters.AddWithValue(
                "@Id",
                id);


            using SqliteDataReader reader =
                findCommand.ExecuteReader();


            if (reader.Read())
            {
                expenseFound =
                    true;


                expenseAmount =
                    reader.GetDouble(0);


                paidFromAccount =
                    reader.IsDBNull(1)
                        ? null
                        : reader.GetString(1);
            }
        }


        if (!expenseFound)
        {
            return;
        }


        // ======================================
        // DELETE EXPENSE
        // ======================================

        using (SqliteCommand deleteCommand =
            new SqliteCommand(
                @"
                DELETE FROM Expenses
                WHERE Id = @Id;
                ",
                connection,
                transaction))
        {
            deleteCommand.Parameters.AddWithValue(
                "@Id",
                id);


            deleteCommand.ExecuteNonQuery();
        }


        // ======================================
        // RESTORE MONEY TO ORIGINAL ACCOUNT
        // ======================================

        AdjustAccountBalance(
            connection,
            transaction,
            paidFromAccount,
            expenseAmount);


        transaction.Commit();
    }

    
    // ==========================================
    // UPDATE EXPENSE
    // ==========================================

    public void UpdateExpense(
        Expense expense)
    {
        using SqliteConnection connection =
            new SqliteConnection(
                connectionString);


        connection.Open();


        using SqliteTransaction transaction =
            connection.BeginTransaction();


        double originalAmount =
            0;


        string? originalPaidFromAccount =
            null;


        bool expenseFound =
            false;


        // ======================================
        // LOAD ORIGINAL EXPENSE
        // ======================================

        using (SqliteCommand findCommand =
            new SqliteCommand(
                @"
                SELECT
                    Amount,
                    PaidFromAccount

                FROM Expenses

                WHERE Id = @Id;
                ",
                connection,
                transaction))
        {
            findCommand.Parameters.AddWithValue(
                "@Id",
                expense.Id);


            using SqliteDataReader reader =
                findCommand.ExecuteReader();


            if (reader.Read())
            {
                expenseFound =
                    true;


                originalAmount =
                    reader.GetDouble(0);


                originalPaidFromAccount =
                    reader.IsDBNull(1)
                        ? null
                        : reader.GetString(1);
            }
        }


        if (!expenseFound)
        {
            return;
        }


        // ======================================
        // UPDATE EXPENSE
        // ======================================

        using (SqliteCommand updateCommand =
            new SqliteCommand(
                @"
                UPDATE Expenses

                SET
                    Name = @Name,
                    Amount = @Amount,
                    Category = @Category,
                    Date = @Date,
                    PaidFromAccount = @PaidFromAccount

                WHERE Id = @Id;
                ",
                connection,
                transaction))
        {
            updateCommand.Parameters.AddWithValue(
                "@Name",
                expense.Name);


            updateCommand.Parameters.AddWithValue(
                "@Amount",
                expense.Amount);


            updateCommand.Parameters.AddWithValue(
                "@Category",
                expense.Category);


            updateCommand.Parameters.AddWithValue(
                "@Date",
                expense.Date.ToString(
                    "yyyy-MM-dd"));


            updateCommand.Parameters.AddWithValue(
                "@PaidFromAccount",
                expense.PaidFromAccount is null
                    ? DBNull.Value
                    : expense.PaidFromAccount);


            updateCommand.Parameters.AddWithValue(
                "@Id",
                expense.Id);


            updateCommand.ExecuteNonQuery();
        }


        // ======================================
        // REVERSE OLD ACCOUNT EFFECT
        // ======================================

        AdjustAccountBalance(
            connection,
            transaction,
            originalPaidFromAccount,
            originalAmount);


        // ======================================
        // APPLY NEW ACCOUNT EFFECT
        // ======================================

        AdjustAccountBalance(
            connection,
            transaction,
            expense.PaidFromAccount,
            -expense.Amount);


        transaction.Commit();
    }


    // ==========================================
    // ADJUST ACCOUNT BALANCE
    // ==========================================

    private void AdjustAccountBalance(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string? accountName,
        double amountChange)
    {
        if (string.IsNullOrWhiteSpace(
                accountName))
        {
            return;
        }


        string? columnName =
            accountName.Equals(
                "Checking",
                StringComparison.OrdinalIgnoreCase)

                ? "CheckingBalance"

                : accountName.Equals(
                    "Cash",
                    StringComparison.OrdinalIgnoreCase)

                    ? "CashBalance"

                    : null;


        // For now PocketAI only allows
        // expenses from Checking or Cash.
        if (columnName == null)
        {
            return;
        }


        string updateAccount = $@"
            UPDATE AccountBalance

            SET {columnName} =
                {columnName} + @AmountChange

            WHERE Id = 1;
        ";


        using SqliteCommand command =
            new SqliteCommand(
                updateAccount,
                connection,
                transaction);


        command.Parameters.AddWithValue(
            "@AmountChange",
            amountChange);


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

    // ==========================================
    // ADD A NEW SAVINGS GOAL
    // ==========================================

    public void AddSavingsGoal(
        SavingsGoal savingsGoal)
    {
        using SqliteConnection connection =
            new SqliteConnection(
                connectionString);


        connection.Open();



        // ======================================
        // FIRST GOAL BECOMES HOME PRIMARY
        // ======================================

        string countGoals =
            @"
            SELECT COUNT(*)
            FROM SavingsGoals;
            ";


        using SqliteCommand countCommand =
            new SqliteCommand(
                countGoals,
                connection);


        long goalCount =
            (long)(
                countCommand.ExecuteScalar()
                ?? 0L);


        bool shouldBePrimary =
            savingsGoal.IsPrimary ||
            goalCount == 0;



        // ======================================
        // DETERMINE PRIORITY
        // ======================================

        int priorityRank =
            savingsGoal.PriorityRank;


        // Current/older UI does not provide
        // a priority yet.
        //
        // Automatically place new goals
        // at the bottom of the priority list.
        if (priorityRank <= 0)
        {
            using SqliteCommand priorityCommand =
                new SqliteCommand(
                    @"
                    SELECT
                        COALESCE(
                            MAX(PriorityRank),
                            0
                        ) + 1

                    FROM SavingsGoals;
                    ",
                    connection);


            object? priorityResult =
                priorityCommand.ExecuteScalar();


            priorityRank =
                Convert.ToInt32(
                    priorityResult ?? 1);
        }



        // ======================================
        // INSERT GOAL
        // ======================================

        string insertGoal =
            @"
            INSERT INTO SavingsGoals
            (
                Name,
                TargetAmount,
                CurrentAmount,
                DeadLine,
                IsPrimary,
                PriorityRank,
                IsEssential,
                CustomAllocationPercentage,
                IsCompleted,
                DateCreated,
                DateCompleted
            )

            VALUES
            (
                @Name,
                @TargetAmount,
                @CurrentAmount,
                @DeadLine,
                @IsPrimary,
                @PriorityRank,
                @IsEssential,
                @CustomAllocationPercentage,
                @IsCompleted,
                @DateCreated,
                @DateCompleted
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
                .ToString(
                    "yyyy-MM-dd"));


        command.Parameters.AddWithValue(
            "@IsPrimary",
            shouldBePrimary
                ? 1
                : 0);


        command.Parameters.AddWithValue(
            "@PriorityRank",
            priorityRank);


        command.Parameters.AddWithValue(
            "@IsEssential",
            savingsGoal.IsEssential
                ? 1
                : 0);


        command.Parameters.AddWithValue(
            "@CustomAllocationPercentage",
            savingsGoal
                .CustomAllocationPercentage
                is null

                    ? DBNull.Value

                    : savingsGoal
                        .CustomAllocationPercentage
                        .Value);


        command.Parameters.AddWithValue(
            "@IsCompleted",
            savingsGoal.IsCompleted
                ? 1
                : 0);


        command.Parameters.AddWithValue(
            "@DateCreated",
            savingsGoal.DateCreated.HasValue

                ? savingsGoal.DateCreated.Value
                    .ToString("O")

                : DBNull.Value);


        command.Parameters.AddWithValue(
            "@DateCompleted",
            savingsGoal.DateCompleted.HasValue

                ? savingsGoal.DateCompleted.Value
                    .ToString("O")

                : DBNull.Value);


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
            new SqliteConnection(
                connectionString);


        connection.Open();



        string selectGoals =
            @"
            SELECT
                Id,
                Name,
                TargetAmount,
                CurrentAmount,
                DeadLine,
                IsPrimary,
                PriorityRank,
                IsEssential,
                CustomAllocationPercentage,
                IsCompleted,
                DateCreated,
                DateCompleted

            FROM SavingsGoals

            ORDER BY
                PriorityRank ASC,
                Id ASC;
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


            int priorityRank =
                reader.GetInt32(6);


            bool isEssential =
                reader.GetInt32(7) == 1;



            double? customAllocationPercentage =
                reader.IsDBNull(8)

                    ? null

                    : reader.GetDouble(8);
            
            bool isCompleted =
                reader.GetInt32(9) == 1;


            DateTime? dateCreated =
                reader.IsDBNull(10)

                    ? null

                    : DateTime.Parse(
                        reader.GetString(10));


            DateTime? dateCompleted =
                reader.IsDBNull(11)

                    ? null

                    : DateTime.Parse(
                        reader.GetString(11));



            SavingsGoal savingsGoal =
                new SavingsGoal(
                    id,
                    name,
                    targetAmount,
                    currentAmount,
                    deadLine,
                    isPrimary,
                    priorityRank,
                    isEssential,
                    customAllocationPercentage);

            savingsGoal.IsCompleted =
                            isCompleted;

            savingsGoal.DateCreated =
                dateCreated;            

            savingsGoal.DateCompleted =
                dateCompleted;

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
            new SqliteConnection(
                connectionString);


        connection.Open();



        string updateGoal =
            @"
            UPDATE SavingsGoals

            SET
                Name =
                    @Name,

                TargetAmount =
                    @TargetAmount,

                CurrentAmount =
                    @CurrentAmount,

                DeadLine =
                    @DeadLine,


                PriorityRank =
                    CASE

                        WHEN @PriorityRank > 0
                        THEN @PriorityRank

                        ELSE PriorityRank

                    END,


                IsEssential =
                    CASE

                        WHEN @PriorityRank > 0
                        THEN @IsEssential

                        ELSE IsEssential

                    END,


                CustomAllocationPercentage =
                    CASE

                        WHEN @PriorityRank > 0
                        THEN @CustomAllocationPercentage

                        ELSE CustomAllocationPercentage

                    END,

                IsCompleted =
                    @IsCompleted,

                DateCreated =
                    @DateCreated,

                DateCompleted =
                    @DateCompleted

            WHERE Id =
                @Id;
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
                .ToString(
                    "yyyy-MM-dd"));


        command.Parameters.AddWithValue(
            "@PriorityRank",
            savingsGoal.PriorityRank);


        command.Parameters.AddWithValue(
            "@IsEssential",
            savingsGoal.IsEssential
                ? 1
                : 0);


        command.Parameters.AddWithValue(
            "@CustomAllocationPercentage",
            savingsGoal
                .CustomAllocationPercentage
                is null

                    ? DBNull.Value

                    : savingsGoal
                        .CustomAllocationPercentage
                        .Value);

        command.Parameters.AddWithValue(
            "@IsCompleted",
            savingsGoal.IsCompleted
                ? 1
                : 0);


        command.Parameters.AddWithValue(
            "@DateCreated",
            savingsGoal.DateCreated.HasValue

                ? savingsGoal.DateCreated.Value
                    .ToString("O")

                : DBNull.Value);


        command.Parameters.AddWithValue(
            "@DateCompleted",
            savingsGoal.DateCompleted.HasValue

                ? savingsGoal.DateCompleted.Value
                    .ToString("O")

                : DBNull.Value);

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

                WHERE Id =
                (
                    SELECT Id

                    FROM SavingsGoals

                    WHERE
                        IsCompleted = 0

                    ORDER BY
                        CASE
                            WHEN PriorityRank <= 0
                                THEN 2147483647

                            ELSE PriorityRank
                        END,

                        Id ASC

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
    // NORMALIZE ACTIVE SAVINGS PRIORITIES
    // ==========================================

    public void NormalizeActiveSavingsGoalPriorities()
    {
        using SqliteConnection connection =
        new SqliteConnection(
            connectionString);


        connection.Open();


        using SqliteTransaction transaction =
            connection.BeginTransaction();


        // ======================================
        // FIND ACTIVE PRIORITY TIERS
        // ======================================

        List<int> priorityLevels =
            new List<int>();


        using (SqliteCommand command =
            new SqliteCommand(
                @"
                SELECT DISTINCT
                    PriorityRank

                FROM SavingsGoals

                WHERE
                    IsCompleted = 0
                    AND
                    PriorityRank > 0

                ORDER BY
                    PriorityRank ASC;
                ",
                connection,
                transaction))
        {
            using SqliteDataReader reader =
                command.ExecuteReader();


            while (reader.Read())
            {
                priorityLevels.Add(
                    reader.GetInt32(0));
            }
        }


        // ======================================
        // COMPACT THE PRIORITY TIERS
        // ======================================

        int newPriorityRank =
            1;


        foreach (int oldPriorityRank
                in priorityLevels)
        {
            using SqliteCommand command =
                new SqliteCommand(
                    @"
                    UPDATE SavingsGoals

                    SET PriorityRank =
                        @NewPriorityRank

                    WHERE
                        IsCompleted = 0
                        AND
                        PriorityRank =
                            @OldPriorityRank;
                    ",
                    connection,
                    transaction);


            command.Parameters.AddWithValue(
                "@NewPriorityRank",
                newPriorityRank);


            command.Parameters.AddWithValue(
                "@OldPriorityRank",
                oldPriorityRank);


            command.ExecuteNonQuery();


            newPriorityRank++;
        }


        transaction.Commit();

    }

    // ==========================================
    // GET PRIMARY ACTIVE SAVINGS GOAL
    // ==========================================
    //
    // Completed goals should never appear as the
    // featured savings goal on Home.
    //
    // If the user's old primary goal was completed,
    // PocketAI falls back to the highest-priority
    // active goal.
    // ==========================================

    public SavingsGoal? GetPrimarySavingsGoal()
    {
        List<SavingsGoal> activeGoals =
            GetSavingsGoals()
                .Where(
                    goal =>
                        !goal.IsCompleted)
                .ToList();


        // ======================================
        // NO ACTIVE GOALS
        // ======================================

        if (activeGoals.Count == 0)
        {
            return null;
        }


        // ======================================
        // EXISTING ACTIVE PRIMARY
        // ======================================

        SavingsGoal? primaryGoal =
            activeGoals.FirstOrDefault(
                goal =>
                    goal.IsPrimary);


        if (primaryGoal != null)
        {
            return primaryGoal;
        }


        // ======================================
        // FALLBACK
        // ======================================
        //
        // If the old primary goal was completed,
        // use the highest-priority active goal.
        // ======================================

        return activeGoals
            .OrderBy(
                goal =>
                    goal.PriorityRank <= 0

                        ? int.MaxValue

                        : goal.PriorityRank)
            .ThenBy(
                goal =>
                    goal.Id)
            .FirstOrDefault();
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

    // Gets all recurring expenses from the database.
    // This includes both active and inactive bills.
    public List<RecurringExpenses> GetRecuringExpenses()
    {
        List<RecurringExpenses> expenses =
            new List<RecurringExpenses>();


        using SqliteConnection connection =
            new SqliteConnection(connectionString);

        connection.Open();


        string query = @"
            SELECT
                Id,
                Name,
                Category,
                Amount,
                DueDay,
                IsActive
            FROM RecurringExpenses
            ORDER BY DueDay ASC;
        ";


        using SqliteCommand command =
            new SqliteCommand(
                query,
                connection);


        using SqliteDataReader reader =
            command.ExecuteReader();


        while (reader.Read())
        {
            RecurringExpenses expense =
                new RecurringExpenses(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetDouble(3),
                    reader.GetInt32(4),
                    reader.GetInt32(5) == 1);


            expenses.Add(
                expense);
        }


        return expenses;
    }

    // ==========================================
    // UPDATE RECURRING EXPENSE
    // ==========================================

    public void UpdateRecurringExpense(
        RecurringExpenses expense)
    {
        using SqliteConnection connection =
            new SqliteConnection(connectionString);

        connection.Open();


        string updateRecurringExpense = @"
            UPDATE RecurringExpenses
            SET
                Name = @Name,
                Category = @Category,
                Amount = @Amount,
                DueDay = @DueDay,
                IsActive = @IsActive
            WHERE Id = @Id;
        ";


        using SqliteCommand command =
            new SqliteCommand(
                updateRecurringExpense,
                connection);


        command.Parameters.AddWithValue(
            "@Name",
            expense.Name);


        command.Parameters.AddWithValue(
            "@Category",
            expense.Category);


        command.Parameters.AddWithValue(
            "@Amount",
            expense.Amount);


        command.Parameters.AddWithValue(
            "@DueDay",
            expense.DueDay);


        command.Parameters.AddWithValue(
            "@IsActive",
            expense.IsActive ? 1 : 0);


        command.Parameters.AddWithValue(
            "@Id",
            expense.Id);


        command.ExecuteNonQuery();
    }


    // ==========================================
    // GET RECURRING BILL PAYMENT FOR MONTH
    // ==========================================

    public RecurringBillPayment?
    GetRecurringBillPayment(
        int recurringExpenseId,
        DateTime month)
    {
        using SqliteConnection connection =
            new SqliteConnection(
                connectionString);


        connection.Open();


        string monthKey =
            month.ToString("yyyy-MM");


        string sql = @"
            SELECT
                Id,
                RecurringExpenseId,
                MonthKey,
                IsPaid,
                DatePaid

            FROM RecurringBillPayments

            WHERE RecurringExpenseId = @RecurringExpenseId
            AND MonthKey = @MonthKey

            LIMIT 1;
        ";


        using SqliteCommand command =
            new SqliteCommand(
                sql,
                connection);


        command.Parameters.AddWithValue(
            "@RecurringExpenseId",
            recurringExpenseId);


        command.Parameters.AddWithValue(
            "@MonthKey",
            monthKey);


        using SqliteDataReader reader =
            command.ExecuteReader();


        if (!reader.Read())
        {
            return null;
        }


        int id =
            Convert.ToInt32(
                reader["Id"]);


        int billId =
            Convert.ToInt32(
                reader["RecurringExpenseId"]);


        string savedMonthKey =
            reader["MonthKey"]?
                .ToString()
            ?? "";


        bool isPaid =
            Convert.ToInt32(
                reader["IsPaid"]) == 1;


        DateTime? datePaid =
            null;


        if (reader["DatePaid"] != DBNull.Value)
        {
            string? datePaidText =
                reader["DatePaid"]?
                    .ToString();


            if (DateTime.TryParse(
                    datePaidText,
                    out DateTime parsedDate))
            {
                datePaid =
                    parsedDate;
            }
        }


        return new RecurringBillPayment(
            id,
            billId,
            savedMonthKey,
            isPaid,
            datePaid);
    }

    //IS RECURRING BILL PAID FOR MONTH
    public bool IsRecurringBillPaidForMonth(
        int recurringExpenseId,
        DateTime month)
    {
        RecurringBillPayment? payment =
            GetRecurringBillPayment(
                recurringExpenseId,
                month);


        return payment?.IsPaid
            ?? false;
    }

    public void SetRecurringBillPaidStatus(
    int recurringExpenseId,
    DateTime month,
    bool isPaid)
    {
        using SqliteConnection connection =
            new SqliteConnection(
                connectionString);


        connection.Open();


        string monthKey =
            month.ToString(
                "yyyy-MM");


        string? datePaid =
            isPaid

                ? DateTime.Now.ToString(
                    "O")

                : null;


        using SqliteCommand command =
            new SqliteCommand(
                @"
                INSERT INTO RecurringBillPayments
                (
                    RecurringExpenseId,
                    MonthKey,
                    IsPaid,
                    DatePaid
                )

                VALUES
                (
                    @RecurringExpenseId,
                    @MonthKey,
                    @IsPaid,
                    @DatePaid
                )

                ON CONFLICT
                (
                    RecurringExpenseId,
                    MonthKey
                )

                DO UPDATE SET

                    IsPaid =
                        excluded.IsPaid,

                    DatePaid =
                        excluded.DatePaid;
                ",
                connection);


        command.Parameters.AddWithValue(
            "@RecurringExpenseId",
            recurringExpenseId);


        command.Parameters.AddWithValue(
            "@MonthKey",
            monthKey);


        command.Parameters.AddWithValue(
            "@IsPaid",
            isPaid
                ? 1
                : 0);


        command.Parameters.AddWithValue(
            "@DatePaid",
            datePaid is null

                ? DBNull.Value

                : datePaid);


        command.ExecuteNonQuery();
    }


    // ==========================================
    // DELETE RECURRING EXPENSE
    // ==========================================

    public void DeleteRecurringExpenseById(
        int id)
    {
        using SqliteConnection connection =
            new SqliteConnection(connectionString);

        connection.Open();


        string deleteRecurringExpense = @"
            DELETE FROM RecurringExpenses
            WHERE Id = @Id;
        ";


        using SqliteCommand command =
            new SqliteCommand(
                deleteRecurringExpense,
                connection);


        command.Parameters.AddWithValue(
            "@Id",
            id);


        command.ExecuteNonQuery();
    }

    // ==========================================
    // GET ACCEPTED EXTRA SAVINGS FOR MONTH
    // ==========================================

    public double GetAcceptedExtraSavingsForMonth(
        DateTime? date = null)
    {
        DateTime targetDate =
            (
                date
                ??
                DateTime.Today
            )
            .Date;


        string monthKey =
            $"{targetDate.Year:D4}-{targetDate.Month:D2}";


        using SqliteConnection connection =
            new SqliteConnection(
                connectionString);


        connection.Open();


        string query = @"
            SELECT Amount
            FROM AcceptedExtraSavings
            WHERE MonthKey = @MonthKey;
        ";


        using SqliteCommand command =
            new SqliteCommand(
                query,
                connection);


        command.Parameters.AddWithValue(
            "@MonthKey",
            monthKey);


        object? result =
            command.ExecuteScalar();


        if (result == null ||
            result == DBNull.Value)
        {
            return 0;
        }


        return Math.Max(
            Convert.ToDouble(
                result),
            0);
    }



    // ==========================================
    // SAVE ACCEPTED EXTRA SAVINGS FOR MONTH
    // ==========================================

    public void SaveAcceptedExtraSavingsForMonth(
        double amount,
        DateTime? date = null)
    {
        DateTime targetDate =
            (
                date
                ??
                DateTime.Today
            )
            .Date;


        string monthKey =
            $"{targetDate.Year:D4}-{targetDate.Month:D2}";


        amount =
            Math.Max(
                amount,
                0);


        using SqliteConnection connection =
            new SqliteConnection(
                connectionString);


        connection.Open();



        // ======================================
        // ZERO MEANS REMOVE THE COMMITMENT
        // ======================================

        if (amount <= 0)
        {
            using SqliteCommand deleteCommand =
                new SqliteCommand(
                    @"
                    DELETE FROM AcceptedExtraSavings
                    WHERE MonthKey = @MonthKey;
                    ",
                    connection);


            deleteCommand.Parameters.AddWithValue(
                "@MonthKey",
                monthKey);


            deleteCommand.ExecuteNonQuery();


            return;
        }



        // ======================================
        // SAVE / REPLACE CURRENT MONTH
        // ======================================

        using SqliteCommand saveCommand =
            new SqliteCommand(
                @"
                INSERT OR REPLACE INTO AcceptedExtraSavings
                (
                    MonthKey,
                    Amount
                )

                VALUES
                (
                    @MonthKey,
                    @Amount
                );
                ",
                connection);


        saveCommand.Parameters.AddWithValue(
            "@MonthKey",
            monthKey);


        saveCommand.Parameters.AddWithValue(
            "@Amount",
            amount);


        saveCommand.ExecuteNonQuery();
    }
}

