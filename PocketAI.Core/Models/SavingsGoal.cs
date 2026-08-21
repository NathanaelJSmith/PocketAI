using System;

public class SavingsGoal
{
    // Unique database ID for this savings goal
    public int Id { get; set; }

    public string Name { get; set; }

    public double TargetAmount { get; set; }

    public double CurrentAmount { get; set; }

    public DateTime DeadLine { get; set; }

    // The primary goal is the one shown on Home
    public bool IsPrimary { get; set; }


    // Keeps old PocketAI code working
    public SavingsGoal(
        string name,
        double targetAmount,
        double currentAmount,
        DateTime deadLine)
    {
        Id = 0;
        Name = name;
        TargetAmount = targetAmount;
        CurrentAmount = currentAmount;
        DeadLine = deadLine;
        IsPrimary = false;
    }


    // Used when loading an existing goal from SQLite
    public SavingsGoal(
        int id,
        string name,
        double targetAmount,
        double currentAmount,
        DateTime deadLine,
        bool isPrimary)
    {
        Id = id;
        Name = name;
        TargetAmount = targetAmount;
        CurrentAmount = currentAmount;
        DeadLine = deadLine;
        IsPrimary = isPrimary;
    }
}