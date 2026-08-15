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


    //Methods for Financial Summary
    //Adds all expenses amounts together
    public double GetTotalSpent(List<Expense> expenses)
    {
        return expenses.Sum(expense => expense.Amount);
    }

    public string GetBiggestSpendingCategory(List<Expense> expenses)
    {
        if (expenses.Count == 0)
        {
            return "";
        }

        return expenses
        .GroupBy(expense => expense.Category)
        .Select(group => new
        {
            Category = group.Key,
            Total = group.Sum(expense => expense.Amount)
        })
        .OrderByDescending(group => group.Total)
        .First()
        .Category;
    }

    //Gets total spending for on category
    public double GetCategoryTotal(List<Expense> expenses, string category)
    {
         return expenses
        .Where(expense =>
            expense.Category.Equals(
                category,
                StringComparison.OrdinalIgnoreCase))
        .Sum(expense => expense.Amount);
    }

    //Builds a financial summary object with all the relevant information
    public FinancialSummary BuildFinancialSummary(
        List<Expense> expenses, 
        Income? userIncome, 
        AccountBalance? userAccountBalance, 
        SavingsGoal? userSavingsGoal, 
        List<BudgetLimit> budgetLimits, 
        List<RecurringExpenses> recurringExpenses)
    {
        FinancialSummary summary = new FinancialSummary();

        // Gets current month expenses
        List<Expense> currentMonthExpenses = GetCurrentMonthExpense(expenses);

        //Gets total spending for the current month
        double totalSpent = GetTotalSpent(currentMonthExpenses);

        //Gets total recurring expenses 
        double monthlyRecurringExpenses = recurringExpenses.Sum(expense => expense.Amount);

        summary.MonthlyRecurringExpenses = monthlyRecurringExpenses;
        summary.RecurringExpenses = recurringExpenses;

        //Adds income information
        if (userIncome != null)
        {
            summary.MonthlyIncome = userIncome.MonthlyAmount;

            //Income minus regular spending minus recurring expenses
            summary.MoneyLeft = userIncome.MonthlyAmount - totalSpent - monthlyRecurringExpenses;
        }

        summary.CurrentMonthSpent = totalSpent;
        
        //Adds account balance information if it exists 
        if (userAccountBalance != null)
        {
            summary.TotalAccountBalance = userAccountBalance.GetTotalBalance();
        }

        //Adds savings goal information if it exists
        if (userSavingsGoal != null)
        {
            summary.SavingsGoalName = userSavingsGoal.Name;
            summary.SavingsTargetAmount = userSavingsGoal.TargetAmount;
            summary.CurrentSavedAmount = userSavingsGoal.CurrentAmount;
            summary.SavingsProgressPercentage = GetSavingsProgressPercentage(summary.CurrentSavedAmount, summary.SavingsTargetAmount);

            summary.SavingsAmountRemaining = userSavingsGoal.TargetAmount - userSavingsGoal.CurrentAmount;

            summary.DaysLeft = (userSavingsGoal.DeadLine - DateTime.Today).TotalDays;  
        

            if (summary.DaysLeft > 0 && summary.SavingsAmountRemaining > 0)
            {
            double weeksLeft = summary.DaysLeft / 7;

            summary.WeeklySavingsNeeded = summary.SavingsAmountRemaining / weeksLeft;
            }
        }

        if (currentMonthExpenses.Count > 0)
        {
            string biggestCategory = GetBiggestSpendingCategory(currentMonthExpenses);

            summary.BiggestSpendingCategory = biggestCategory;

            summary.BiggestCategoryAmount = GetCategoryTotal(currentMonthExpenses, biggestCategory);
        }

        //Counts how many budget categories have been over budget
        foreach(BudgetLimit limit in budgetLimits)
        {
            double categoryTotal = GetCategoryTotal(currentMonthExpenses, limit.Category);

            if(categoryTotal > limit.LimitAmount)
            {
                summary.OverBudgetCount++;
            }
        }
    
        return summary;
    }
    
    //Calculates how many days until a recurring expense is due
    public int GetDaysUntilDue(int dueDay)
    {
        DateTime today = DateTime.Today;
        DateTime dueDate;

        //Bill is still coming up this month
        if(today.Day <= dueDay)
        {
            int validDueDay = Math.Min(dueDay, DateTime.DaysInMonth(today.Year, today.Month));

            dueDate = new DateTime(today.Year, today.Month, validDueDay);
        }
        else
        {
            DateTime nextMonth = today.AddMonths(1);

            int validDueDay = Math.Min(dueDay, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));

            dueDate = new DateTime(nextMonth.Year, nextMonth.Month, validDueDay);
        }

        return (dueDate - today).Days;
    }

    public int GetFinancialHealthScore(
        FinancialSummary summary,
        double projectedEndOfMonthMoney, double safeToSpend)
    {

        int score = 100;

        //No income 
        if (summary.MonthlyIncome <= 0)
        {
            score -= 30;
        }

        //End of Month cash flow
        if (projectedEndOfMonthMoney < 0)
        {
            score -= 30;
        }
        else if (summary.MonthlyIncome > 0)
        {
            double endOfMonthRatio = projectedEndOfMonthMoney / summary.MonthlyIncome;

            if (endOfMonthRatio < 0.10)
            {
                score -= 15;
            }
            else if (endOfMonthRatio < 0.20)
            {
                score -= 8;
            }
        }
        //OverBudget categories
        int budgetPenalty = Math.Min(summary.OverBudgetCount * 5, 20);

        
        score -= budgetPenalty;

        if (summary.MonthlyIncome > 0)
        {
            double recurringRatio = summary.MonthlyRecurringExpenses / summary.MonthlyIncome;

            if (recurringRatio > 0.5)
            {
                score -= 15;
            }
            else if (recurringRatio > 0.3)
            {
                score -= 8;
            }
        }

        //Savings Progress
        if (summary.SavingsTargetAmount > 0)
        {
            double savingsProgress = summary.CurrentSavedAmount / summary.SavingsTargetAmount;

            if (savingsProgress < 0.10)
            {
                score -= 10;
            }
            else if (savingsProgress < 0.50)
            {
                score -= 5;
            }
        }

        if (safeToSpend < 0)
        {
            score -= 30;
        }
        else if (summary.MonthlyIncome > 0)
        {
            double safeToSpendRatio = safeToSpend / summary.MonthlyIncome;

            if (safeToSpendRatio < 0.05)
            {
                score -= 20;
            }
            else if (safeToSpendRatio < 0.10)
            {
                score -= 15;
            }
            else if (safeToSpendRatio < 0.20)
            {
                score -= 10;
            }
        }
       
       return Math.Clamp(score, 0, 100);

    }
    
    public string GetFinancialHealthStatus(int score)
    {
        if(score >= 90)
        {
            return "Excellent financial health! Keep up the good work.";
        }
        else if(score >= 75)
        {
            return "Good financial health. You're on the right track.";
        }
        else if(score >= 60)
        {
            return "Fair financial health. Consider reviewing your budget and spending habits.";
        }
        else if(score >= 40)
        {
            return "Poor financial health. Immediate action is recommended to improve your finances.";
        }
        else
        {
            return "Very poor financial health. Seek professional financial advice and take urgent steps to improve your situation.";
        }
    }


    //Calculates savings goal progress as a percentage
    public double GetSavingsNeededThisMonth(
        double savingsAmountRemaining,
        double daysUntilDeadline,
        int daysInMonth)
    {
        if(savingsAmountRemaining <= 0 || daysUntilDeadline <= 0 || daysInMonth <= 0)
        {
            return 0;
        }
        
        //Calculates how much needs to be saved per day
        double dailySavingsNeeded = savingsAmountRemaining / daysUntilDeadline;

        //Only reserves savings for days remaing in this month
        double savingDays = Math.Min(daysUntilDeadline, daysInMonth);

        double savingsNeededThisMonth = dailySavingsNeeded * savingDays;

        return Math.Min(savingsNeededThisMonth, savingsAmountRemaining);
            
        
    }

    public double GetSavingsProgressPercentage(double currentSavedAmount, double savingsTargetAmount)
    {
        if (savingsTargetAmount <= 0)
        {
            return 0;
        }

        double percentage = (currentSavedAmount / savingsTargetAmount) * 100;
        return Math.Clamp(percentage, 0, 100);
    }

}