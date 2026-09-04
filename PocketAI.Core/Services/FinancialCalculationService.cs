using System;
using System.Collections.Generic;
using System.Linq;


// ==========================================
// POCKETAI CENTRAL FINANCIAL ENGINE
// ==========================================
// IMPORTANT DEFINITIONS:
//
// MONTHLY INCOME
// = expected income for the month.
//
// TRANSACTIONS
// = spending recorded during the month.
//
// BILLS
// = obligations that reduce the monthly plan.
//
// REQUIRED SAVINGS
// = savings needed to keep goals on pace.
//
// ACCEPTED EXTRA SAVINGS
// = optional additional savings the user
//   explicitly chooses.
//
// SAFE TO SPEND / AVAILABLE TO SPEND
// = expected monthly income
//   - spending
//   - bills
//   - required savings
//   - accepted extra savings.
//
// CHECKING
// = actual money currently in checking.
//
// SAVINGS ACCOUNT
// = actual money currently in savings.
//
// SAVINGS GOALS
// = what saved money is intended for.
//
// CASH
// = optional physical cash.
//
// Account balances answer:
// "Where is my money right now?"
//
// The monthly plan answers:
// "How much can I still spend this month?"
// ==========================================

public class FinancialCalculationService
{
    // ==========================================
    // BUILD COMPLETE FINANCIAL SNAPSHOT
    // ==========================================

    public FinancialSnapshot BuildSnapshot(
        List<Expense> expenses,
        Income? income,
        AccountBalance? accountBalance,
        List<SavingsGoal> savingsGoals,
        List<BudgetLimit> budgetLimits,
        List<RecurringExpenses> recurringExpenses,
        double acceptedExtraSavings = 0,
        DateTime? asOfDate = null)
    {
        // ======================================
        // SAFETY FALLBACKS
        // ======================================

        expenses ??=
            new List<Expense>();


        savingsGoals ??=
            new List<SavingsGoal>();


        budgetLimits ??=
            new List<BudgetLimit>();


        recurringExpenses ??=
            new List<RecurringExpenses>();



        DateTime today =
            (
                asOfDate
                ??
                DateTime.Today
            )
            .Date;



        int daysInMonth =
            DateTime.DaysInMonth(
                today.Year,
                today.Month);



        // Includes today.
        //
        // Example:
        //
        // August 27
        // August has 31 days
        //
        // Days available:
        //
        // 27, 28, 29, 30, 31
        //
        // = 5 days
        int daysLeftInMonth =
            Math.Max(
                daysInMonth -
                today.Day +
                1,
                1);



        // ======================================
        // CURRENT MONTH TRANSACTIONS
        // ======================================

        List<Expense> currentMonthExpenses =
            expenses
                .Where(
                    expense =>
                        expense.Date.Year ==
                        today.Year
                        &&
                        expense.Date.Month ==
                        today.Month)
                .ToList();



        double currentMonthSpent =
            currentMonthExpenses
                .Sum(
                    expense =>
                        Math.Max(
                            expense.Amount,
                            0));

        // ======================================
        // ACCOUNT-LINKED SPENDING THIS MONTH
        // ======================================
        //
        // Expenses paid from Checking or Cash
        // already reduced the user's CURRENT
        // account balances.
        //
        // We track those expenses separately so
        // PocketAI can reconstruct the amount of
        // spendable cash that existed before the
        // current month's tracked spending.
        //
        // This keeps the safety buffer stable as
        // the user spends money.
        //
        // IMPORTANT:
        //
        // Older transactions with no Paid From
        // account are intentionally ignored here.
        // They did not reduce an account balance,
        // so adding them back would inflate cash.
        // ======================================

        double accountLinkedCurrentMonthSpent =
            currentMonthExpenses
                .Where(
                    expense =>
                        string.Equals(
                            expense.PaidFromAccount,
                            "Checking",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        string.Equals(
                            expense.PaidFromAccount,
                            "Cash",
                            StringComparison.OrdinalIgnoreCase))
                .Sum(
                    expense =>
                        Math.Max(
                            expense.Amount,
                            0));

        // ======================================
        // CURRENT ACCOUNT BALANCES
        // ======================================

        double checkingBalance =
            accountBalance?
                .CheckingBalance
            ??
            0;


        double savingsBalance =
            accountBalance?
                .SavingsBalance
            ??
            0;


        double cashBalance =
            accountBalance?
                .CashBalance
            ??
            0;



        // ======================================
        // CURRENT SPENDABLE CASH
        // ======================================
        //
        // Savings is intentionally NOT included.
        //
        // Checking + Cash are considered
        // available spending sources.
        //
        // Example:
        //
        // Checking = $308.29
        // Cash     = $200
        //
        // Current Spendable Cash = $508.29
        // ======================================

        double currentSpendableCash =
            checkingBalance +
            cashBalance;



        double totalAccountBalance =
            checkingBalance +
            savingsBalance +
            cashBalance;



        // ======================================
        // EXPECTED MONTHLY INCOME
        // ======================================
        //
        // This is planning data.
        //
        // It does NOT automatically increase
        // current spendable cash.
        // ======================================

        double expectedMonthlyIncome =
            Math.Max(
                income?
                    .MonthlyAmount
                ??
                0,
                0);



        // ======================================
        // ACTIVE RECURRING BILLS
        // ======================================

        List<RecurringExpenses>
            activeRecurringBills =
                recurringExpenses
                    .Where(
                        bill =>
                            bill.IsActive)
                    .ToList();



        double monthlyRecurringExpenses =
            activeRecurringBills
                .Sum(
                    bill =>
                        Math.Max(
                            bill.Amount,
                            0));



        // ======================================
        // UPCOMING BILLS
        // ======================================
        //
        // IMPORTANT:
        //
        // The current RecurringExpenses model
        // does not yet track whether a bill was
        // actually paid.
        //
        // To avoid double-counting money that
        // has already left the user's CURRENT
        // account balance, PocketAI currently
        // protects bills due TODAY OR LATER
        // this month.
        //
        // Later we should add actual bill-payment
        // tracking.
        // ======================================

        double upcomingBills =
            CalculateUpcomingBills(
                activeRecurringBills,
                today);



        // ======================================
        // ACTIVE SAVINGS GOALS
        // ======================================

        List<SavingsGoal>
            activeSavingsGoals =
                savingsGoals
                    .Where(
                        goal =>
                            goal.CurrentAmount
                            <
                            goal.TargetAmount)
                    .ToList();



        // ======================================
        // REQUIRED SAVINGS THIS MONTH
        // ======================================
        //
        // This is NOT PocketAI's optional
        // recommendation.
        //
        // This is only the minimum contribution
        // required between now and month-end to
        // remain on pace toward goal deadlines.
        // ======================================

        double requiredSavingsThisMonth =
            CalculateRequiredSavingsThisMonth(
                activeSavingsGoals,
                today,
                daysLeftInMonth);



        // ======================================
        // MONTHLY PLANNING REMAINDER
        // ======================================
        //
        // This answers:
        //
        // "Based on what I expect to earn this
        // month, how much room is left in my
        // monthly financial plan?"
        //
        // IMPORTANT:
        //
        // Expected income is PLANNING information.
        //
        // It does NOT get added to current
        // Checking or Cash and therefore does
        // NOT directly increase Safe to Spend.
        //
        // We subtract:
        //
        // - money already spent this month
        // - bills still expected this month
        // - required savings
        // - optional savings already accepted
        //
        // ======================================

        double planningAcceptedExtraSavings =
            Math.Max(
                acceptedExtraSavings,
                0);


        double monthlyPlanRemaining =
            expectedMonthlyIncome
            -
            currentMonthSpent
            -
            upcomingBills
            -
            requiredSavingsThisMonth
            -
            planningAcceptedExtraSavings;



        // ======================================
        // OVER-BUDGET COUNT
        // ======================================

        int overBudgetCount =
            CalculateOverBudgetCount(
                currentMonthExpenses,
                budgetLimits);



        // ======================================
        // DATA CONFIDENCE
        // ======================================

        (
            string dataConfidence,
            string dataConfidenceReason
        ) =
            CalculateDataConfidence(
                accountBalance,
                income,
                currentMonthExpenses.Count,
                budgetLimits.Count,
                activeRecurringBills.Count);



        // ======================================
        // ACCEPTED EXTRA SAVINGS
        // ======================================
        //
        // PocketAI suggestions DO NOT reduce
        // Safe to Spend automatically.
        //
        // Only an amount the USER accepts is
        // protected here.
        // ======================================

        acceptedExtraSavings =
            planningAcceptedExtraSavings;



        // ======================================
        // KNOWN OBLIGATIONS
        // ======================================

        double knownObligations =
            upcomingBills
            +
            requiredSavingsThisMonth
            +
            acceptedExtraSavings;



        // ======================================
        // OBLIGATION SHORTFALL
        // ======================================
        //
        // Safety buffer does NOT count as a
        // required obligation.
        // ======================================

        double obligationShortfall =
            Math.Max(
                -monthlyPlanRemaining,
                0);



        // ======================================
        // MONEY AFTER OBLIGATIONS
        // ======================================

        double availableBeforeBuffer =
            currentSpendableCash
            -
            knownObligations;



        // ======================================
        // SAFETY BUFFER BASIS
        // ======================================
        //
        // The buffer should stay stable when the
        // user accepts optional extra savings.
        //
        // Accepted extra savings is a USER CHOICE.
        // It should reduce Safe to Spend dollar
        // for dollar.
        //
        // Therefore:
        //
        // Bills + Required Savings
        // affect the basis used to establish the
        // safety cushion.
        //
        // Accepted Extra Savings does NOT shrink
        // that cushion.
        // ======================================

        double bufferProtectedObligations =
            upcomingBills
            +
            requiredSavingsThisMonth;



        double safetyBufferBasis =
            Math.Max(
                currentSpendableCash
                +
                accountLinkedCurrentMonthSpent
                -
                bufferProtectedObligations,
                0);



        // ======================================
        // SAFETY BUFFER
        // ======================================

        double safetyBuffer =
            CalculateSafetyBuffer(
                safetyBufferBasis,
                dataConfidence);



        // ======================================
        // TOTAL SAFE TO SPEND
        // ======================================

        double safeToSpendTotal =
            Math.Max(
                monthlyPlanRemaining,
                0);



        // ======================================
        // SAFE TO SPEND TODAY
        // ======================================

        double safeToSpendToday =
            0;


        if (daysLeftInMonth > 0 &&
            safeToSpendTotal > 0)
        {
            double calculatedDailyAmount =
                safeToSpendTotal
                /
                daysLeftInMonth;


            // Hard protection:
            //
            // Today's amount can never exceed
            // total Safe to Spend.
            safeToSpendToday =
                Math.Min(
                    safeToSpendTotal,
                    calculatedDailyAmount);
        }



        // ======================================
        // SAFE TO SPEND THIS WEEK
        // ======================================

        int daysAvailableThisWeek =
            Math.Min(
                7,
                daysLeftInMonth);



        double safeToSpendThisWeek =
            safeToSpendToday
            *
            daysAvailableThisWeek;



        // HARD CAP.
        //
        // This directly prevents bugs like:
        //
        // Total Safe to Spend = $1,042
        // Weekly Safe to Spend = $1,460
        //
        // Weekly can NEVER exceed total.
        safeToSpendThisWeek =
            Math.Min(
                safeToSpendTotal,
                safeToSpendThisWeek);



        // ======================================
        // AVERAGE DAILY SPENDING
        // ======================================

        int daysElapsed =
            Math.Max(
                today.Day,
                1);



        double averageDailySpending =
            currentMonthSpent
            /
            daysElapsed;



        // ======================================
        // PROJECT FUTURE TRANSACTION SPENDING
        // ======================================
        //
        // We project tomorrow through the end
        // of the month.
        //
        // Today's recorded transactions are
        // already included in currentMonthSpent.
        // ======================================

        int futureDays =
            Math.Max(
                daysInMonth -
                today.Day,
                0);



        double projectedAdditionalSpending =
            averageDailySpending
            *
            futureDays;


        double projectedMonthlyPlanRemaining =
            monthlyPlanRemaining
            -
            projectedAdditionalSpending;

        // ======================================
        // PROJECTED MONTH-END SPENDABLE CASH
        // ======================================
        //
        // Starts with CURRENT spendable cash.
        //
        // NOT monthly income.
        //
        // Safety buffer is not subtracted here
        // because the buffer remains cash.
        // ======================================

        double projectedMonthEndSpendableCash =
            currentSpendableCash
            -
            upcomingBills
            -
            requiredSavingsThisMonth
            -
            acceptedExtraSavings
            -
            projectedAdditionalSpending;



        // ======================================
        // OPTIONAL POCKETAI EXTRA SAVINGS
        // ======================================
        //
        // This recommendation is intentionally
        // conservative.
        //
        // LOW confidence:
        //
        // PocketAI does NOT recommend extra
        // savings yet.
        //
        // MEDIUM:
        //
        // Up to 20% of Safe to Spend.
        //
        // HIGH:
        //
        // Up to 30%.
        //
        // This recommendation DOES NOT reduce
        // Safe to Spend until accepted.
        // ======================================

        double pocketAiRecommendedExtraSavings =
            CalculateRecommendedExtraSavings(
                safeToSpendTotal,
                projectedMonthlyPlanRemaining,
                activeSavingsGoals.Count,
                dataConfidence);



        // ======================================
        // FINANCIAL HEALTH AVAILABILITY
        // ======================================
        //
        // Do not manufacture a score when the
        // user has almost no financial history.
        // ======================================

        bool hasEnoughDataForHealthScore =
            accountBalance != null
            &&
            income != null
            &&
            currentMonthExpenses.Count >= 3;



        int? financialHealthScore =
            null;



        if (hasEnoughDataForHealthScore)
        {
            financialHealthScore =
                CalculateFinancialHealthScore(
                    currentSpendableCash,
                    safeToSpendTotal,
                    obligationShortfall,
                    monthlyPlanRemaining,
                    projectedMonthEndSpendableCash,
                    overBudgetCount,
                    dataConfidence);
        }



        // ======================================
        // BUILD FINAL SNAPSHOT
        // ======================================

        FinancialSnapshot snapshot =
            new FinancialSnapshot
            {
                // CURRENT MONEY

                CheckingBalance =
                    Math.Round(
                        checkingBalance,
                        2),


                CashBalance =
                    Math.Round(
                        cashBalance,
                        2),


                ProtectedSavingsBalance =
                    Math.Round(
                        savingsBalance,
                        2),


                CurrentSpendableCash =
                    Math.Round(
                        currentSpendableCash,
                        2),


                TotalAccountBalance =
                    Math.Round(
                        totalAccountBalance,
                        2),



                // MONTHLY PLANNING

                ExpectedMonthlyIncome =
                    Math.Round(
                        expectedMonthlyIncome,
                        2),


                CurrentMonthSpent =
                    Math.Round(
                        currentMonthSpent,
                        2),


                MonthlyPlanRemaining =
                    Math.Round(
                        monthlyPlanRemaining,
                        2),



                // OBLIGATIONS

                UpcomingBills =
                    Math.Round(
                        upcomingBills,
                        2),


                RequiredSavingsThisMonth =
                    Math.Round(
                        requiredSavingsThisMonth,
                        2),


                PocketAiRecommendedExtraSavings =
                    Math.Round(
                        pocketAiRecommendedExtraSavings,
                        2),


                AcceptedExtraSavings =
                    Math.Round(
                        acceptedExtraSavings,
                        2),



                // SAFETY

                SafetyBuffer =
                    Math.Round(
                        safetyBuffer,
                        2),


                ObligationShortfall =
                    Math.Round(
                        obligationShortfall,
                        2),



                // SAFE TO SPEND

                SafeToSpendTotal =
                    Math.Round(
                        safeToSpendTotal,
                        2),


                SafeToSpendToday =
                    Math.Round(
                        safeToSpendToday,
                        2),


                SafeToSpendThisWeek =
                    Math.Round(
                        safeToSpendThisWeek,
                        2),



                // PROJECTIONS

                AverageDailySpending =
                    Math.Round(
                        averageDailySpending,
                        2),


                ProjectedAdditionalSpending =
                    Math.Round(
                        projectedAdditionalSpending,
                        2),


                ProjectedMonthEndSpendableCash =
                    Math.Round(
                        projectedMonthEndSpendableCash,
                        2),



                // BUDGETS

                OverBudgetCount =
                    overBudgetCount,


                BudgetCount =
                    budgetLimits.Count,



                // DATA QUALITY

                CurrentMonthTransactionCount =
                    currentMonthExpenses.Count,


                ActiveRecurringBillCount =
                    activeRecurringBills.Count,


                ActiveSavingsGoalCount =
                    activeSavingsGoals.Count,


                DataConfidence =
                    dataConfidence,


                DataConfidenceReason =
                    dataConfidenceReason,


                HasEnoughDataForHealthScore =
                    hasEnoughDataForHealthScore,


                FinancialHealthScore =
                    financialHealthScore
            };



        return snapshot;
    }



    // ==========================================
    // UPCOMING BILLS
    // ==========================================

    private double CalculateUpcomingBills(
        List<RecurringExpenses> recurringBills,
        DateTime today)
    {
        double total =
            0;



        int daysInMonth =
            DateTime.DaysInMonth(
                today.Year,
                today.Month);



        foreach (RecurringExpenses bill
                 in recurringBills)
        {
            if (!bill.IsActive)
            {
                continue;
            }



            // Protect against invalid saved
            // due-day values.
            int dueDay =
                Math.Clamp(
                    bill.DueDay,
                    1,
                    daysInMonth);



            // Due dates that already passed are
            // currently assumed to have already
            // been handled by the current account
            // balance.
            //
            // This changes once PocketAI tracks
            // actual bill payments.
            if (dueDay <
                today.Day)
            {
                continue;
            }



            total +=
                Math.Max(
                    bill.Amount,
                    0);
        }



        return total;
    }



    // ==========================================
    // REQUIRED SAVINGS THIS MONTH
    // ==========================================

    private double CalculateRequiredSavingsThisMonth(
        List<SavingsGoal> savingsGoals,
        DateTime today,
        int daysLeftInMonth)
    {
        double totalRequired =
            0;



        DateTime endOfMonth =
            new DateTime(
                today.Year,
                today.Month,
                DateTime.DaysInMonth(
                    today.Year,
                    today.Month));



        foreach (SavingsGoal goal
                 in savingsGoals)
        {
            double remaining =
                Math.Max(
                    goal.TargetAmount
                    -
                    goal.CurrentAmount,
                    0);



            if (remaining <= 0)
            {
                continue;
            }



            // A custom 0% allocation means the
            // user intentionally paused this goal.
            if (goal
                    .CustomAllocationPercentage
                    .HasValue
                &&
                goal
                    .CustomAllocationPercentage
                    .Value <= 0)
            {
                continue;
            }



            double daysUntilDeadline =
                (
                    goal.DeadLine.Date
                    -
                    today.Date
                )
                .TotalDays;



            // Past-due goals no longer have a
            // valid schedule.
            //
            // We do NOT suddenly reserve their
            // entire balance from Safe to Spend.
            //
            // The UI should instead tell the user
            // to update the deadline.
            if (daysUntilDeadline < 0)
            {
                continue;
            }



            // Due today.
            if (daysUntilDeadline == 0)
            {
                totalRequired +=
                    remaining;


                continue;
            }



            // Deadline occurs during the current
            // month.
            //
            // The entire remaining amount must be
            // funded this month to remain on pace.
            if (goal.DeadLine.Date <=
                endOfMonth)
            {
                totalRequired +=
                    remaining;


                continue;
            }



            // ==================================
            // PRO-RATE THROUGH MONTH END
            // ==================================
            //
            // Example:
            //
            // $1,100 remaining
            //
            // 35 days until target
            //
            // 5 days left this month
            //
            // $1,100 × (5 / 35)
            //
            // = about $157.14 required during
            // the remainder of this month.
            // ==================================

            double requiredForRemainingMonth =
                remaining
                *
                (
                    daysLeftInMonth
                    /
                    daysUntilDeadline
                );



            requiredForRemainingMonth =
                Math.Min(
                    requiredForRemainingMonth,
                    remaining);



            totalRequired +=
                Math.Max(
                    requiredForRemainingMonth,
                    0);
        }



        return totalRequired;
    }



    // ==========================================
    // OVER-BUDGET COUNT
    // ==========================================

    private int CalculateOverBudgetCount(
        List<Expense> currentMonthExpenses,
        List<BudgetLimit> budgetLimits)
    {
        int overBudgetCount =
            0;



        foreach (BudgetLimit budget
                 in budgetLimits)
        {
            double categorySpent =
                currentMonthExpenses
                    .Where(
                        expense =>
                            expense.Category.Equals(
                                budget.Category,
                                StringComparison.OrdinalIgnoreCase))
                    .Sum(
                        expense =>
                            Math.Max(
                                expense.Amount,
                                0));



            if (categorySpent >
                budget.LimitAmount)
            {
                overBudgetCount++;
            }
        }



        return overBudgetCount;
    }



    // ==========================================
    // DATA CONFIDENCE
    // ==========================================

    private (
        string Confidence,
        string Reason
    )
    CalculateDataConfidence(
        AccountBalance? accountBalance,
        Income? income,
        int transactionCount,
        int budgetCount,
        int recurringBillCount)
    {
        // ======================================
        // LOW CONFIDENCE
        // ======================================

        if (accountBalance == null)
        {
            return
            (
                "Low",
                "Add your current account balances so PocketAI knows how much money actually exists right now."
            );
        }



        if (transactionCount < 3)
        {
            return
            (
                "Low",
                "PocketAI needs more transaction history before it can confidently judge your spending behavior."
            );
        }



        // ======================================
        // MEDIUM CONFIDENCE
        // ======================================

        if (income == null)
        {
            return
            (
                "Medium",
                "PocketAI can see your current cash and spending history, but adding expected monthly income will improve planning."
            );
        }



        if (transactionCount < 10)
        {
            return
            (
                "Medium",
                "PocketAI has enough information for basic analysis, but more transaction history will make trends more reliable."
            );
        }



        if (budgetCount == 0 &&
            recurringBillCount == 0)
        {
            return
            (
                "Medium",
                "PocketAI has good transaction history, but adding budgets or recurring bills will improve future-obligation planning."
            );
        }



        // ======================================
        // HIGH CONFIDENCE
        // ======================================

        return
        (
            "High",
            "PocketAI has current balances, income, transaction history, and planning information available."
        );
    }



    // ==========================================
    // SAFETY BUFFER
    // ==========================================

    private double CalculateSafetyBuffer(
        double safeteyBufferBasis,
        string dataConfidence)
    {
        if (safeteyBufferBasis <= 0)
        {
            return 0;
        }



        double percentage;



        if (dataConfidence.Equals(
                "Low",
                StringComparison.OrdinalIgnoreCase))
        {
            percentage =
                0.15;
        }


        else if (dataConfidence.Equals(
                     "Medium",
                     StringComparison.OrdinalIgnoreCase))
        {
            percentage =
                0.12;
        }


        else
        {
            percentage =
                0.10;
        }



        return
            safeteyBufferBasis
            *
            percentage;
    }



    // ==========================================
    // OPTIONAL EXTRA SAVINGS RECOMMENDATION
    // ==========================================

    private double CalculateRecommendedExtraSavings(
        double safeToSpendTotal,
        double projectedMonthlyPlanRemaining,
        int activeSavingsGoalCount,
        string dataConfidence)
    {
        //Nothing Availaibe
        if (safeToSpendTotal <= 0 || activeSavingsGoalCount <= 0)
        {
            return 0;
        }

        //Monthly income plan connot support
        if(projectedMonthlyPlanRemaining <= 0)
        {
            return 0;
        }

        //Low Confidence
        if (dataConfidence.Equals(
                "Low",
                StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        //Recommended percentage
        double percantage;

        if (dataConfidence.Equals(
            "High",
            StringComparison.OrdinalIgnoreCase))
        {
            percantage = 0.30;
        }
        else
        {
            percantage = 0.20;
        }
        
        //current cahs recommendation
        double recommendationFromSafeCash = safeToSpendTotal * percantage;

        double recommendationFromIncomePlan = projectedMonthlyPlanRemaining * percantage;

        return Math.Min(recommendationFromSafeCash, recommendationFromIncomePlan);
    }



    // ==========================================
    // FINANCIAL HEALTH SCORE
    // ==========================================

    private int CalculateFinancialHealthScore(
        double currentSpendableCash,
        double safeToSpendTotal,
        double obligationShortfall,
        double monthlyPlanRemaining,
        double projectedMonthEndSpendableCash,
        int overBudgetCount,
        string dataConfidence)
    {
        // Start neutral rather than assuming
        // perfect financial health.
        int score =
            70;



        // ======================================
        // OBLIGATIONS
        // ======================================

        if (obligationShortfall > 0)
        {
            score -=
                25;
        }
        else
        {
            score +=
                10;
        }



        // ======================================
        // MONTH-END PROJECTION
        // ======================================

        if (projectedMonthEndSpendableCash < 0)
        {
            score -=
                20;
        }
        else
        {
            score +=
                10;
        }



        // ======================================
        // MONTHLY PLAN
        // ======================================

        if (monthlyPlanRemaining < 0)
        {
            score -=
                15;
        }
        else
        {
            score +=
                5;
        }



        // ======================================
        // BUDGET PERFORMANCE
        // ======================================

        score -=
            Math.Min(
                overBudgetCount * 8,
                24);



        // ======================================
        // AVAILABLE CASH
        // ======================================

        if (currentSpendableCash <= 0)
        {
            score -=
                15;
        }



        if (safeToSpendTotal <= 0)
        {
            score -=
                10;
        }
        else
        {
            score +=
                5;
        }



        score =
            Math.Clamp(
                score,
                0,
                100);



        // Medium-confidence information should
        // not produce an overly confident
        // "95 / 100" style result.
        if (dataConfidence.Equals(
                "Medium",
                StringComparison.OrdinalIgnoreCase))
        {
            score =
                Math.Min(
                    score,
                    89);
        }



        return score;
    }
}