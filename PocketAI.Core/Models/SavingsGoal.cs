using System;

public class SavingsGoal
{
    // ==========================================
    // BASIC GOAL INFORMATION
    // ==========================================

    // Unique database ID.
    public int Id { get; set; }


    // Name of the goal.
    //
    // Examples:
    // Emergency Fund
    // Vacation
    // Laptop
    public string Name { get; set; }


    // Total amount the user wants to save.
    public double TargetAmount { get; set; }


    // Amount of Savings Account money currently
    // assigned to this goal.
    //
    // This does NOT represent additional money
    // outside of the Savings Account.
    //
    // Assigning money to a goal does NOT change
    // the Savings Account balance.
    public double CurrentAmount { get; set; }


    // ==========================================
    // GOAL COMPLETION
    // ==========================================

    // True when the user has officially chosen
    // to finish/archive the goal.
    //
    // Reaching the target does NOT automatically
    // set this to true because the user may choose
    // to increase the target instead.
    public bool IsCompleted {get; set;}

    // Date this goal was originally created.
    //
    // Nullable so older PocketAI goals do not get
    // a fake creation date during database migration.
    public DateTime? DateCreated
    {
        get;
        set;
    }


    // Date the user officially pressed
    // Finish Goal.
    //
    // null means the goal has not been completed.
    public DateTime? DateCompleted
    {
        get;
        set;
    }

    // Automatically tells PocketAI whether the
    // goal has reached its target amount.
    //
    // This is NOT the same as IsCompleted.
    //
    // Example:
    //
    // CurrentAmount = $2,000
    // TargetAmount  = $2,000
    //
    // IsTargetReached = true
    //
    // But IsCompleted remains false until
    // the user presses Finish Goal.
    public bool IsTargetReached
    {
        get
        {
            return CurrentAmount >= TargetAmount;
        }
    }
    // Date the user wants to complete the goal.
    public DateTime DeadLine { get; set; }



    // ==========================================
    // HOME PAGE PRIMARY GOAL
    // ==========================================

    // IsPrimary is NOT the same thing
    // as savings priority.
    //
    // IsPrimary only determines which
    // savings goal is featured on Home.
    public bool IsPrimary { get; set; }



    // ==========================================
    // PRIORITY SAVINGS
    // ==========================================

    // Financial importance of this goal.
    //
    // 1 = highest priority
    // 2 = second priority
    // 3 = third priority
    // etc.
    //
    // A value of 0 means the goal has not
    // been ranked yet. The database will
    // automatically assign a rank.
    public int PriorityRank { get; set; }


    // Identifies goals that should receive
    // additional protection when money is tight.
    //
    // Examples:
    //
    // Emergency Fund = true
    // Necessary Car Repair = true
    // Vacation = false
    // Gaming PC = false
    public bool IsEssential { get; set; }


    // ==========================================
    // USER-CONTROLLED ALLOCATION
    // ==========================================

    // null:
    // PocketAI calculates the recommended
    // percentage automatically.
    //
    // number:
    // The user manually controls what
    // percentage of savings goes to this goal.
    //
    // Example:
    // 50 = 50%
    // 25 = 25%
    //
    // Using nullable double is important because
    // 0% can intentionally mean:
    // "Temporarily pause this goal."
    //
    // null means:
    // "Use PocketAI's recommendation."
    public double? CustomAllocationPercentage
    {
        get;
        set;
    }



    // ==========================================
    // ORIGINAL CONSTRUCTOR
    // ==========================================

    // Keeps all existing PocketAI code working.
    //
    // New goals created with the current UI
    // will automatically receive their
    // PriorityRank from the database.
    public SavingsGoal(
        string name,
        double targetAmount,
        double currentAmount,
        DateTime deadLine)
    {
        Id =
            0;

        Name =
            name;

        TargetAmount =
            targetAmount;

        CurrentAmount =
            currentAmount;

        DeadLine =
            deadLine;
        
        IsCompleted = false;

        DateCreated = DateTime.Now;

        DateCompleted = null;

        IsPrimary =
            false;


        // Database will automatically
        // assign the next available rank.
        PriorityRank =
            0;


        // Optional by default until
        // the user says otherwise.
        IsEssential =
            false;


        // null means PocketAI recommendation.
        CustomAllocationPercentage =
            null;
    }



    // ==========================================
    // ORIGINAL DATABASE CONSTRUCTOR
    // ==========================================

    // Keeps existing PocketAI code working
    // while we transition to Priority Savings.
    public SavingsGoal(
        int id,
        string name,
        double targetAmount,
        double currentAmount,
        DateTime deadLine,
        bool isPrimary)
    {
        Id =
            id;

        Name =
            name;

        TargetAmount =
            targetAmount;

        CurrentAmount =
            currentAmount;

        DeadLine =
            deadLine;

        IsCompleted =
            false;

        DateCreated = DateTime.Now;

        DateCompleted = null;

        IsPrimary =
            isPrimary;


        PriorityRank =
            0;

        IsEssential =
            false;

        CustomAllocationPercentage =
            null;
    }



    // ==========================================
    // PRIORITY SAVINGS CONSTRUCTOR
    // ==========================================

    // This constructor will be used by the
    // upgraded database and Savings page.
    public SavingsGoal(
        int id,
        string name,
        double targetAmount,
        double currentAmount,
        DateTime deadLine,
        bool isPrimary,
        int priorityRank,
        bool isEssential,
        double? customAllocationPercentage)
    {
        Id =
            id;

        Name =
            name;

        TargetAmount =
            targetAmount;

        CurrentAmount =
            currentAmount;

        DeadLine =
            deadLine;

        IsPrimary =
            isPrimary;

        IsCompleted = false;

        DateCreated = DateTime.Now;

        DateCompleted = null;

        PriorityRank =
            priorityRank;

        IsEssential =
            isEssential;

        CustomAllocationPercentage =
            customAllocationPercentage;
    }
}