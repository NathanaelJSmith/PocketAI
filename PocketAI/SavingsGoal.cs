using System;


class SavingsGoal
{

    public string Name {  get; set; }

    public double TargetAmount { get; set; }

    public double CurrentAmount { get; set; }

    public DateTime DeadLine { get; set; }

    public SavingsGoal(string name, double targetAmount, double currentAmount, DateTime deadLine)
    {
        Name = name;
        TargetAmount = targetAmount;
        CurrentAmount = currentAmount;
        DeadLine = deadLine;
    }
}
