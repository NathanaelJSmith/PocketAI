using System;

// Holds the main financial numbers for PocketAI
class FinancialSummary
{
    // Stores monthly income
    public double MonthlyIncome { get; set; }

    // Stores current month spending
    public double CurrentMonthSpent { get; set; }

    // Stores money left after spending
    public double MoneyLeft { get; set; }

    // Stores total account balance
    public double TotalAccountBalance { get; set; }

    // Stores savings goal name
    public string SavingsGoalName { get; set; }

    // Stores savings target amount
    public double SavingsTargetAmount { get; set; }

    // Stores current saved amount
    public double CurrentSavedAmount { get; set; }

    // Stores amount still needed for savings goal
    public double SavingsAmountRemaining { get; set; }

    // Stores days left until savings deadline
    public double DaysLeft { get; set; }

    // Stores weekly savings needed
    public double WeeklySavingsNeeded { get; set; }

    // Stores biggest spending category
    public string BiggestSpendingCategory { get; set; }

    // Stores amount spent in biggest category
    public double BiggestCategoryAmount { get; set; }

    // Stores number of budget categories over limit
    public int OverBudgetCount { get; set; }
}
