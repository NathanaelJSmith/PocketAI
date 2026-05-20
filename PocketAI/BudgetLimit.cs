using System;


class BudgetLimit
{
    public string Category {  get; set; }

    public double LimitAmount { get; set; }

    public BudgetLimit(string category, double limitAmount)
    {
        Category = category;
        LimitAmount = limitAmount;
    }
}