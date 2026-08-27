using System;


// ==========================================
// POCKETAI FINANCIAL SNAPSHOT
// ==========================================
//
// This object represents PocketAI's single,
// consistent view of the user's finances.
//
// Pages should DISPLAY these values.
//
// Pages should NOT invent their own versions
// of Safe to Spend, money remaining,
// financial health, or savings protection.
//
// FinancialCalculationService will calculate
// this snapshot in the next step.
// ==========================================

public class FinancialSnapshot
{
    // ==========================================
    // REAL MONEY AVAILABLE NOW
    // ==========================================

    // Current checking-account balance.
    public double CheckingBalance
    {
        get;
        set;
    }


    // Current physical cash balance.
    public double CashBalance
    {
        get;
        set;
    }


    // Money held in savings.
    //
    // This is protected by default and should
    // NOT automatically be considered spendable.
    public double ProtectedSavingsBalance
    {
        get;
        set;
    }


    // Checking + cash.
    //
    // This is the starting point for
    // CURRENT Safe to Spend calculations.
    public double CurrentSpendableCash
    {
        get;
        set;
    }


    // Checking + savings + cash.
    //
    // Useful for showing total account value,
    // but NOT for calculating Safe to Spend.
    public double TotalAccountBalance
    {
        get;
        set;
    }



    // ==========================================
    // MONTHLY PLANNING
    // ==========================================

    // Expected monthly income.
    //
    // This is planning information.
    //
    // It must NOT automatically be treated as
    // money currently available to spend.
    public double ExpectedMonthlyIncome
    {
        get;
        set;
    }


    // Expenses recorded during this month.
    //
    // This is historical/activity information.
    public double CurrentMonthSpent
    {
        get;
        set;
    }


    // Monthly planning calculation:
    //
    // Expected income
    // - recorded monthly spending
    // - planned recurring expenses
    //
    // This replaces the misleading meaning
    // previously attached to "MoneyLeft".
    //
    // It is NOT current spendable cash.
    public double MonthlyPlanRemaining
    {
        get;
        set;
    }



    // ==========================================
    // CURRENT OBLIGATIONS
    // ==========================================

    // Active recurring bills that still need
    // protection from current available cash.
    public double UpcomingBills
    {
        get;
        set;
    }


    // Minimum savings contribution needed
    // during the remaining part of this month
    // to keep active goals on schedule.
    public double RequiredSavingsThisMonth
    {
        get;
        set;
    }


    // Optional extra amount PocketAI believes
    // the user could save beyond the minimum.
    //
    // IMPORTANT:
    // This does NOT reduce Safe to Spend merely
    // because PocketAI recommended it.
    public double PocketAiRecommendedExtraSavings
    {
        get;
        set;
    }


    // Extra savings amount the USER has actually
    // chosen to include in their plan.
    //
    // Only accepted savings should reduce
    // Safe to Spend.
    public double AcceptedExtraSavings
    {
        get;
        set;
    }



    // ==========================================
    // SAFETY
    // ==========================================

    // Current cash PocketAI recommends leaving
    // untouched as a cushion.
    public double SafetyBuffer
    {
        get;
        set;
    }


    // If current obligations exceed current
    // spendable cash, this records the shortage.
    public double ObligationShortfall
    {
        get;
        set;
    }



    // ==========================================
    // SAFE TO SPEND
    // ==========================================

    // CurrentSpendableCash
    // - UpcomingBills
    // - RequiredSavingsThisMonth
    // - AcceptedExtraSavings
    // - SafetyBuffer
    //
    // Never below zero for display purposes.
    public double SafeToSpendTotal
    {
        get;
        set;
    }


    // Portion of SafeToSpendTotal available
    // today.
    //
    // Never greater than SafeToSpendTotal.
    public double SafeToSpendToday
    {
        get;
        set;
    }


    // Portion of SafeToSpendTotal available
    // over the next seven days or fewer if the
    // month ends sooner.
    //
    // Never greater than SafeToSpendTotal.
    public double SafeToSpendThisWeek
    {
        get;
        set;
    }



    // ==========================================
    // PROJECTIONS
    // ==========================================

    // Average transaction spending per day
    // during the current month.
    public double AverageDailySpending
    {
        get;
        set;
    }


    // Estimated additional spending from now
    // through the end of the month.
    public double ProjectedAdditionalSpending
    {
        get;
        set;
    }


    // Projection beginning with CURRENT
    // spendable cash, not expected monthly
    // income.
    public double ProjectedMonthEndSpendableCash
    {
        get;
        set;
    }



    // ==========================================
    // BUDGET INFORMATION
    // ==========================================

    public int OverBudgetCount
    {
        get;
        set;
    }


    public int BudgetCount
    {
        get;
        set;
    }



    // ==========================================
    // DATA QUALITY
    // ==========================================

    public int CurrentMonthTransactionCount
    {
        get;
        set;
    }


    public int ActiveRecurringBillCount
    {
        get;
        set;
    }


    public int ActiveSavingsGoalCount
    {
        get;
        set;
    }


    // Low / Medium / High
    public string DataConfidence
    {
        get;
        set;
    } = "Low";


    // Plain-English reason shown to the user.
    public string DataConfidenceReason
    {
        get;
        set;
    } = "";


    // PocketAI should not manufacture a
    // confident health score when there is
    // insufficient information.
    public bool HasEnoughDataForHealthScore
    {
        get;
        set;
    }


    // null means:
    //
    // "Not enough data yet"
    //
    // rather than pretending the user has an
    // excellent or poor financial-health score.
    public int? FinancialHealthScore
    {
        get;
        set;
    }
}