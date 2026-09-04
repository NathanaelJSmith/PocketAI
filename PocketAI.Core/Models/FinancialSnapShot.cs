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
    // This represents actual spendable money
    // currently held by the user.
    //
    // It answers:
    // "How much money do I physically have
    // available in checking and cash right now?"
    //
    // It does NOT determine the monthly
    // Safe to Spend calculation.
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


    // Monthly budgeting calculation:
    //
    // Expected Monthly Income
    // - Current Month Spending
    // - Bills
    // - Required Savings
    // - Accepted Extra Savings
    //
    // This answers:
    //
    // "How much of my monthly plan is
    // still available?"
    //
    // This is separate from actual
    // checking, savings, and cash balances.
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


    // Amount by which the monthly plan
    // is short.
    //
    // Example:
    //
    // Income = $2,000
    // Spending + Bills + Required Savings
    // = $2,200
    //
    // ObligationShortfall = $200.
    //
    // This represents a monthly planning
    // shortage, not an account-balance shortage.
    public double ObligationShortfall
    {
        get;
        set;
    }



    // ==========================================
    // SAFE TO SPEND
    // ==========================================

    // Available to Spend / Safe to Spend:
    //
    // Expected Monthly Income
    // - Current Month Spending
    // - Bills
    // - Required Savings
    // - Accepted Extra Savings
    //
    // SafeToSpendTotal is the amount remaining
    // in the user's MONTHLY budget.
    //
    // It does NOT represent the current
    // checking-account balance.
    //
    // Never below zero for display purposes.
    // Any shortage is stored separately in
    // ObligationShortfall.
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