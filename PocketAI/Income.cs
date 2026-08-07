using System;

public class Income
{
    public string Source { get; set; }

    public double MonthlyAmount { get; set; }

    public Income (string source, double monthlyAmount)
    {
        Source = source;
        MonthlyAmount = monthlyAmount;
    }

}
