using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public class AnalyticsService
{
    // Gets expenses from the current week
    public List<Expense> GetCurrentWeekExpenses(List<Expense> expenses)
    {
        DateTime today = DateTime.Today;

        // Treats Monday as the beginning of the week
        int difference =
            (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;

        DateTime startOfWeek = today.AddDays(-difference);

        return expenses
            .Where(expense => expense.Date >= startOfWeek)
            .ToList();
    }
    // Gets expenses from the previous week
    public List<Expense> GetLastWeekExpenses(List<Expense> expenses)
    {
        DateTime today = DateTime.Today;

        int difference =
            (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;

        DateTime startOfCurrentWeek = today.AddDays(-difference);
        DateTime startOfLastWeek = startOfCurrentWeek.AddDays(-7);

        return expenses
            .Where(expense =>
                expense.Date >= startOfLastWeek &&
                expense.Date < startOfCurrentWeek)
            .ToList();
    }

    //gets expenses from current Month
    public List<Expense> GetCurrentMonthExpense(List<Expense> expenses)
    {
        DateTime today = DateTime.Today;
        return expenses
            .Where(expense => expense.Date.Month == today.Month && expense.Date.Year == today.Year)
            .ToList();
    }
    //gets expenses form last month
    public List<Expense> GetLastMonthExpense(List<Expense> expenses)
    {
        DateTime today = DateTime.Today;

    DateTime firstDayOfCurrentMonth =
        new DateTime(today.Year, today.Month, 1);

    DateTime firstDayOfLastMonth =
        firstDayOfCurrentMonth.AddMonths(-1);

    return expenses
        .Where(expense =>
            expense.Date >= firstDayOfLastMonth &&
            expense.Date < firstDayOfCurrentMonth)
        .ToList();
    }

    //Calculates average daily spending for the current month
    public double GetAverageDailySpending(double currentMonthSpent, int dayPassed)
    {
        if (dayPassed <= 0 )
        {
            return 0;
        }

        return currentMonthSpent / dayPassed;
    }

    //Estimates how much more the user may spend this month
    public double GetProjectedAdditionalSpending(double averageDailySpending, int daysLefts)
    {
        if (daysLefts <= 0)
        {
            return 0;
        }

        return averageDailySpending * daysLefts;
    }

    //Estimates how much money will remain at the end of the month
    public double GetProjectedEndOfMonthMoney(double moneyLeft, double projectedAdditionalSpending)
    {
        return moneyLeft - projectedAdditionalSpending;

    }

   //Calculates how much money is safe to spend after savings
   public double GetSafeToSpend(double moneyLeft, double savingsNeeded)
    {
        return moneyLeft - savingsNeeded;
    }

    //Calculates the daily safe-to-spend amount
    public double GetDailySafeToSpend(double safeToSpend, int daysLeft)
    {
        if(daysLeft <= 0)
        {
            return 0;
        }

        return safeToSpend / daysLeft;
    }

    //Calculates the weekly safe-to-spend amount
    public double GetWeeklySafeToSpend(double safeToSpend, double weeksLeft)
    {
        if (weeksLeft <= 0)
        {
            return 0;
        }

        return safeToSpend / weeksLeft;
    }

    //Calcualates the spending differences between two periods
    public double GetSpendingDifference(double currentSpending, double previousSpending)
    {
        return currentSpending - previousSpending;
    }

    //Calculates the percentage change between two periods
    public double GetSpendingPercentageChange(double currentSpending, double previousSpending)
    {
        //Prevents division by 0
        if (previousSpending <= 0)
        {
            return 0;
        }

        return ((currentSpending - previousSpending) / previousSpending) * 100;
    }
}