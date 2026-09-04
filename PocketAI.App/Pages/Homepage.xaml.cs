using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PocketAI.App.Pages;

public partial class HomePage : ContentPage
{
    // ==========================================
    // DATABASE
    // ==========================================

    private readonly DataBaseManager
        dataBaseManager;


    // ==========================================
    // CENTRAL FINANCIAL ENGINE
    // ==========================================

    private readonly FinancialSnapshotProvider
        financialSnapshotProvider;



    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public HomePage()
    {
        InitializeComponent();


        string databasePath =
            Path.Combine(
                FileSystem.AppDataDirectory,
                "pocketai.db");


        dataBaseManager =
            new DataBaseManager(
                databasePath);


        dataBaseManager.CreateTables();


        // Home no longer calculates its own
        // Safe to Spend or Financial Health.
        //
        // It gets one trusted snapshot from
        // the centralized financial engine.
        financialSnapshotProvider =
            new FinancialSnapshotProvider(
                dataBaseManager);
    }



    // ==========================================
    // PAGE APPEARS
    // ==========================================

    protected override void OnAppearing()
    {
        base.OnAppearing();


        LoadFinancialData();
    }



    // ==========================================
    // LOAD HOME
    // ==========================================

    private void LoadFinancialData()
    {
        // ======================================
        // CENTRAL FINANCIAL SNAPSHOT
        // ======================================
        //
        // THIS is now the source of truth for:
        //
        // - current spendable cash
        // - total balance
        // - expected income
        // - spending
        // - upcoming obligations
        // - required savings
        // - safety buffer
        // - Safe to Spend
        // - daily Safe to Spend
        // - weekly Safe to Spend
        // - projections
        // - data confidence
        // - financial health
        //
        // Home does NOT recalculate these.
        // ======================================

        FinancialSnapshot snapshot =
            financialSnapshotProvider
                .GetSnapshot();



        // ======================================
        // LOAD PRESENTATION DATA
        // ======================================
        //
        // These records are still needed to
        // display individual goals, budgets,
        // bills, and insights.
        //
        // They are NOT used to create a second
        // version of Safe to Spend.
        // ======================================

        List<Expense> expenses =
            dataBaseManager
                .GetAllExpenses();


        AccountBalance? accountBalance =
            dataBaseManager
                .GetAccountBalance();


        SavingsGoal? savingsGoal =
            dataBaseManager
                .GetSavingsGoal();


        List<BudgetLimit> budgetLimits =
            dataBaseManager
                .GetBudgetLimits();


        List<RecurringExpenses>
            recurringExpenses =
                dataBaseManager
                    .GetRecuringExpenses();



        DateTime today =
            DateTime.Today;



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



        // ======================================
        // UPDATE PAGE
        // ======================================

        UpdateGreeting();


        UpdateSafeToSpend(
            snapshot);


        UpdateFinancialHealth(
            snapshot);


        UpdateMoneyCards(
            snapshot);


        UpdateSavingsGoal(
            savingsGoal,
            today);


        UpdateBudgetSnapshot(
            budgetLimits,
            currentMonthExpenses);


        UpdateUpcomingBills(
            recurringExpenses,
            today);


        UpdatePocketAiInsight(
            snapshot,
            accountBalance,
            savingsGoal,
            currentMonthExpenses,
            today);
    }



    // ==========================================
    // GREETING
    // ==========================================

    private void UpdateGreeting()
    {
        int hour =
            DateTime.Now.Hour;



        if (hour < 12)
        {
            GreetingLabel.Text =
                "Good morning";
        }
        else if (hour < 17)
        {
            GreetingLabel.Text =
                "Good afternoon";
        }
        else
        {
            GreetingLabel.Text =
                "Good evening";
        }
    }



    // ==========================================
    // SAFE TO SPEND
    // ==========================================

    private void UpdateSafeToSpend(
        FinancialSnapshot snapshot)
    {

        // ======================================
        // CONFIDENCE-AWARE TITLE
        // ======================================

        if (snapshot.DataConfidence.Equals(
                "Low",
                StringComparison.OrdinalIgnoreCase))
        {
            SafeToSpendTitleLabel.Text =
                "ESTIMATED SAFE TO SPEND";
        }
        else
        {
            SafeToSpendTitleLabel.Text =
                "SAFE TO SPEND";
        }
        // ======================================
        // TOTAL SAFE TO SPEND
        // ======================================

        SafeToSpendTotalLabel.Text =
            snapshot
                .SafeToSpendTotal
                .ToString("C");



        // ======================================
        // TODAY
        // ======================================

        DailySafeToSpendLabel.Text =
            snapshot
                .SafeToSpendToday
                .ToString("C");


        // ======================================
        // WEEKLY LABEL
        // ======================================
        //
        // If fewer than seven days remain in the
        // month, show the actual end date instead
        // of misleadingly saying "This Week".
        // ======================================

        DateTime today =
            DateTime.Today;


        int daysInMonth =
            DateTime.DaysInMonth(
                today.Year,
                today.Month);


        int daysLeftInMonth =
            daysInMonth -
            today.Day +
            1;


        if (daysLeftInMonth < 7)
        {
            DateTime endOfMonth =
                new DateTime(
                    today.Year,
                    today.Month,
                    daysInMonth);


            WeeklySafeToSpendTitleLabel.Text =
                $"THROUGH {endOfMonth:MMM d}"
                    .ToUpper();
        }
        else
        {
            WeeklySafeToSpendTitleLabel.Text =
                "THIS WEEK";
}
        // ======================================
        // THIS WEEK
        // ======================================

        WeeklySafeToSpendLabel.Text =
            snapshot
                .SafeToSpendThisWeek
                .ToString("C");



        // ======================================
        // OBLIGATION SHORTFALL
        // ======================================

        if (snapshot.ObligationShortfall > 0)
        {
            SafeToSpendStatusLabel.Text =
                $"⚠ Your monthly plan is short by " +
                $"{snapshot.ObligationShortfall:C}.";


            SafeToSpendStatusLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "DangerColor");


            return;
        }



        // ======================================
        // NO DISCRETIONARY ROOM
        // ======================================

        if (snapshot.SafeToSpendTotal <= 0)
        {
            SafeToSpendStatusLabel.Text =
                "No discretionary spending room right now.";


            SafeToSpendStatusLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "WarningColor");


            return;
        }



        // ======================================
        // ON TRACK
        // ======================================

        if (snapshot.DataConfidence.Equals(
                "Low",
                StringComparison.OrdinalIgnoreCase))
        {
            SafeToSpendStatusLabel.Text =
                "✓ No known shortfall based on current data";
        }
        else
        {
            SafeToSpendStatusLabel.Text =
                "✓ You're currently on track";
}


        SafeToSpendStatusLabel
            .SetDynamicResource(
                Label.TextColorProperty,
                "SuccessColor");
    }



    // ==========================================
    // FINANCIAL HEALTH
    // ==========================================

    private void UpdateFinancialHealth(
        FinancialSnapshot snapshot)
    {
        // ======================================
        // NOT ENOUGH DATA
        // ======================================

        if (!snapshot.HasEnoughDataForHealthScore ||
            !snapshot.FinancialHealthScore.HasValue)
        {
            FinancialHealthScoreLabel.Text =
                "Not enough data";


            FinancialHealthScoreLabel.FontSize =
                25;


            FinancialHealthStatusLabel.Text =
                $"{snapshot.DataConfidence} confidence";


            FinancialHealthReasonLabel.Text =
                snapshot.DataConfidenceReason;



            if (snapshot.DataConfidence.Equals(
                    "Low",
                    StringComparison.OrdinalIgnoreCase))
            {
                FinancialHealthStatusLabel
                    .SetDynamicResource(
                        Label.TextColorProperty,
                        "WarningColor");
            }
            else
            {
                FinancialHealthStatusLabel
                    .SetDynamicResource(
                        Label.TextColorProperty,
                        "ThemePrimary");
            }


            return;
        }



        // ======================================
        // SCORE AVAILABLE
        // ======================================

        int score =
            snapshot
                .FinancialHealthScore
                .Value;



        FinancialHealthScoreLabel.FontSize =
            38;


        FinancialHealthScoreLabel.Text =
            $"{score} / 100";


        FinancialHealthStatusLabel.Text =
            $"{GetFinancialHealthStatus(score)} • " +
            $"{snapshot.DataConfidence} confidence";


        FinancialHealthReasonLabel.Text =
            snapshot.DataConfidenceReason;



        // ======================================
        // SCORE COLOR
        // ======================================

        if (score >= 70)
        {
            FinancialHealthStatusLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "SuccessColor");
        }
        else if (score >= 50)
        {
            FinancialHealthStatusLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "WarningColor");
        }
        else
        {
            FinancialHealthStatusLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "DangerColor");
        }
    }



    // ==========================================
    // FINANCIAL HEALTH STATUS TEXT
    // ==========================================

    private string GetFinancialHealthStatus(
        int score)
    {
        if (score >= 85)
        {
            return
                "Strong financial position";
        }


        if (score >= 70)
        {
            return
                "Generally healthy";
        }


        if (score >= 50)
        {
            return
                "Needs attention";
        }


        return
            "High financial pressure";
    }



    // ==========================================
    // YOUR MONEY
    // ==========================================

    private void UpdateMoneyCards(
        FinancialSnapshot snapshot)
    {
        // ======================================
        // TOTAL ACCOUNT VALUE
        // ======================================

        TotalBalanceLabel.Text =
            snapshot
                .TotalAccountBalance
                .ToString("C");



        // ======================================
        // CURRENT SPENDABLE CASH
        // ======================================
        //
        // Checking + Cash
        //
        // Savings remains protected.
        // ======================================

        SpendableCashLabel.Text =
            snapshot
                .CurrentSpendableCash
                .ToString("C");



        // ======================================
        // EXPECTED INCOME
        // ======================================
        //
        // Planning information only.
        //
        // This does NOT mean this money
        // currently exists in the account.
        // ======================================

        MonthlyIncomeLabel.Text =
            snapshot
                .ExpectedMonthlyIncome
                .ToString("C");



        // ======================================
        // MONTHLY SPENDING
        // ======================================

        MonthlySpentLabel.Text =
            snapshot
                .CurrentMonthSpent
                .ToString("C");
    }



    // ==========================================
    // HOME SAVINGS GOAL
    // ==========================================

    private void UpdateSavingsGoal(
        SavingsGoal? savingsGoal,
        DateTime today)
    {
        // ======================================
        // NO GOAL
        // ======================================

        if (savingsGoal == null)
        {
            SavingsGoalNameLabel.Text =
                "No active goal";


            SavingsProgressAmountLabel.Text =
                "$0.00 / $0.00";


            SavingsProgressPercentLabel.Text =
                "0%";


            SavingsProgressBar.Progress =
                0;


            SavingsRemainingLabel.Text =
                "$0.00 remaining";


            WeeklySavingsNeededLabel.Text =
                "$0.00/week needed";


            SavingsStatusLabel.Text =
                "No savings goal";


            SavingsStatusLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "TextSecondary");


            return;
        }



        // ======================================
        // AMOUNTS
        // ======================================

        double remaining =
            Math.Max(
                savingsGoal.TargetAmount
                -
                savingsGoal.CurrentAmount,
                0);



        double progressPercentage =
            0;



        if (savingsGoal.TargetAmount > 0)
        {
            progressPercentage =
                Math.Clamp(
                    savingsGoal.CurrentAmount
                    /
                    savingsGoal.TargetAmount
                    *
                    100,
                    0,
                    100);
        }



        double daysUntilDeadline =
            (
                savingsGoal.DeadLine.Date
                -
                today.Date
            )
            .TotalDays;



        double weeklyNeeded =
            CalculateWeeklySavingsNeeded(
                savingsGoal,
                today);



        // ======================================
        // DISPLAY
        // ======================================

        SavingsGoalNameLabel.Text =
            savingsGoal.Name;


        SavingsProgressAmountLabel.Text =
            $"{savingsGoal.CurrentAmount:C} / " +
            $"{savingsGoal.TargetAmount:C}";


        SavingsProgressPercentLabel.Text =
            $"{progressPercentage:F0}%";


        SavingsProgressBar.Progress =
            progressPercentage
            /
            100.0;


        SavingsRemainingLabel.Text =
            $"{remaining:C} remaining";



        // ======================================
        // COMPLETED
        // ======================================

        if (remaining <= 0)
        {
            WeeklySavingsNeededLabel.Text =
                "Goal fully funded";


            SavingsStatusLabel.Text =
                "✓ Goal complete";


            SavingsStatusLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "SuccessColor");


            return;
        }



        // ======================================
        // DEADLINE REACHED
        // ======================================

        if (daysUntilDeadline <= 0)
        {
            WeeklySavingsNeededLabel.Text =
                "Update your target date";


            SavingsStatusLabel.Text =
                "⚠ Target date reached";


            SavingsStatusLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "DangerColor");


            return;
        }



        // ======================================
        // ACTIVE GOAL
        // ======================================

        WeeklySavingsNeededLabel.Text =
            $"{weeklyNeeded:C}/week needed";


        int displayDays =
            Math.Max(
                (int)Math.Ceiling(
                    daysUntilDeadline),
                0);


        SavingsStatusLabel.Text =
            $"{displayDays} days remaining";


        SavingsStatusLabel
            .SetDynamicResource(
                Label.TextColorProperty,
                "SuccessColor");
    }



    // ==========================================
    // WEEKLY SAVINGS REQUIREMENT
    // ==========================================

    private double CalculateWeeklySavingsNeeded(
        SavingsGoal goal,
        DateTime today)
    {
        double remaining =
            Math.Max(
                goal.TargetAmount
                -
                goal.CurrentAmount,
                0);



        if (remaining <= 0)
        {
            return 0;
        }



        double daysUntilDeadline =
            (
                goal.DeadLine.Date
                -
                today.Date
            )
            .TotalDays;



        if (daysUntilDeadline <= 0)
        {
            return 0;
        }



        double weeksLeft =
            daysUntilDeadline
            /
            7.0;



        if (weeksLeft <= 0)
        {
            return 0;
        }



        return
            remaining
            /
            weeksLeft;
    }



    // ==========================================
    // BUDGET SNAPSHOT
    // ==========================================

    private void UpdateBudgetSnapshot(
        List<BudgetLimit> budgetLimits,
        List<Expense> currentMonthExpenses)
    {
        BudgetSnapshotContainer
            .Children
            .Clear();



        // ======================================
        // NO BUDGETS
        // ======================================

        if (budgetLimits.Count == 0)
        {
            BudgetEmptyLabel.IsVisible =
                true;


            BudgetEmptyLabel.Text =
                "No budget information yet.";


            return;
        }



        BudgetEmptyLabel.IsVisible =
            false;



        // ======================================
        // TOP THREE BUDGETS
        // ======================================

        foreach (BudgetLimit budget
                 in budgetLimits.Take(3))
        {
            double spent =
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



            double remaining =
                budget.LimitAmount
                -
                spent;



            double progress =
                0;



            if (budget.LimitAmount > 0)
            {
                progress =
                    spent
                    /
                    budget.LimitAmount;
            }



            double displayedProgress =
                Math.Clamp(
                    progress,
                    0,
                    1);



            // ==================================
            // TOP ROW
            // ==================================

            Grid topRow =
                new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(
                            GridLength.Star),

                        new ColumnDefinition(
                            GridLength.Auto)
                    }
                };



            Label categoryLabel =
                new Label
                {
                    Text =
                        budget.Category,

                    FontSize =
                        14,

                    FontAttributes =
                        FontAttributes.Bold
                };


            categoryLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "TextPrimary");



            Label amountLabel =
                new Label
                {
                    Text =
                        $"{spent:C} / " +
                        $"{budget.LimitAmount:C}",

                    FontSize =
                        13
                };


            amountLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "TextSecondary");



            Grid.SetColumn(
                categoryLabel,
                0);


            Grid.SetColumn(
                amountLabel,
                1);



            topRow.Children.Add(
                categoryLabel);


            topRow.Children.Add(
                amountLabel);



            // ==================================
            // PROGRESS BAR
            // ==================================

            ProgressBar progressBar =
                new ProgressBar
                {
                    Progress =
                        displayedProgress,

                    HeightRequest =
                        8
                };



            if (remaining < 0)
            {
                progressBar
                    .SetDynamicResource(
                        ProgressBar
                            .ProgressColorProperty,
                        "DangerColor");
            }
            else
            {
                progressBar
                    .SetDynamicResource(
                        ProgressBar
                            .ProgressColorProperty,
                        "ThemePrimary");
            }



            progressBar
                .SetDynamicResource(
                    ProgressBar
                        .BackgroundColorProperty,
                    "BorderColor");



            // ==================================
            // REMAINING MESSAGE
            // ==================================

            Label remainingLabel =
                new Label
                {
                    FontSize =
                        12
                };



            if (remaining >= 0)
            {
                remainingLabel.Text =
                    $"{remaining:C} left";


                remainingLabel
                    .SetDynamicResource(
                        Label.TextColorProperty,
                        "TextSecondary");
            }
            else
            {
                remainingLabel.Text =
                    $"{Math.Abs(remaining):C} OVER BUDGET";


                remainingLabel
                    .SetDynamicResource(
                        Label.TextColorProperty,
                        "DangerColor");


                remainingLabel.FontAttributes =
                    FontAttributes.Bold;
            }



            VerticalStackLayout budgetRow =
                new VerticalStackLayout
                {
                    Spacing =
                        6
                };



            budgetRow.Children.Add(
                topRow);


            budgetRow.Children.Add(
                progressBar);


            budgetRow.Children.Add(
                remainingLabel);



            BudgetSnapshotContainer
                .Children
                .Add(
                    budgetRow);
        }
    }



    // ==========================================
    // UPCOMING BILLS
    // ==========================================

    private void UpdateUpcomingBills(
        List<RecurringExpenses> recurringExpenses,
        DateTime today)
    {
        UpcomingBillsContainer
            .Children
            .Clear();



        // ======================================
        // FIND NEXT DUE DATE FOR EACH BILL
        // ======================================

        var activeBills =
            recurringExpenses
                .Where(
                    bill =>
                        bill.IsActive)
                .Select(
                    bill =>
                        new
                        {
                            Bill =
                                bill,

                            DueDate =
                                GetNextDueDate(
                                    bill.DueDay,
                                    today)
                        })
                .OrderBy(
                    item =>
                        item.DueDate)
                .Take(3)
                .ToList();



        // ======================================
        // NO BILLS
        // ======================================

        if (activeBills.Count == 0)
        {
            BillsEmptyLabel.IsVisible =
                true;


            BillsEmptyLabel.Text =
                "No upcoming bills.";


            return;
        }



        BillsEmptyLabel.IsVisible =
            false;



        // ======================================
        // DISPLAY BILLS
        // ======================================

        foreach (var item
                 in activeBills)
        {
            RecurringExpenses bill =
                item.Bill;


            DateTime dueDate =
                item.DueDate;


            int daysUntilDue =
                Math.Max(
                    (
                        dueDate.Date
                        -
                        today.Date
                    )
                    .Days,
                    0);



            Grid billRow =
                new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(
                            GridLength.Star),

                        new ColumnDefinition(
                            GridLength.Auto)
                    },

                    ColumnSpacing =
                        20
                };



            VerticalStackLayout billInfo =
                new VerticalStackLayout
                {
                    Spacing =
                        3
                };



            Label nameLabel =
                new Label
                {
                    Text =
                        bill.Name,

                    FontSize =
                        14,

                    FontAttributes =
                        FontAttributes.Bold
                };


            nameLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "TextPrimary");



            Label categoryLabel =
                new Label
                {
                    Text =
                        bill.Category,

                    FontSize =
                        12
                };


            categoryLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "TextSecondary");



            string dueDescription =
                daysUntilDue switch
                {
                    0 =>
                        "Due today",

                    1 =>
                        "Due tomorrow",

                    _ =>
                        $"Due in {daysUntilDue} days"
                };



            Label dueLabel =
                new Label
                {
                    Text =
                        $"Due {dueDate:MMM d} • " +
                        dueDescription,

                    FontSize =
                        12
                };


            dueLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "TextSecondary");



            billInfo.Children.Add(
                nameLabel);


            billInfo.Children.Add(
                categoryLabel);


            billInfo.Children.Add(
                dueLabel);



            Label amountLabel =
                new Label
                {
                    Text =
                        bill.Amount.ToString("C"),

                    FontSize =
                        15,

                    FontAttributes =
                        FontAttributes.Bold,

                    VerticalOptions =
                        LayoutOptions.Center
                };


            amountLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "TextPrimary");



            Grid.SetColumn(
                billInfo,
                0);


            Grid.SetColumn(
                amountLabel,
                1);



            billRow.Children.Add(
                billInfo);


            billRow.Children.Add(
                amountLabel);



            UpcomingBillsContainer
                .Children
                .Add(
                    billRow);
        }
    }



    // ==========================================
    // NEXT BILL DUE DATE
    // ==========================================

    private DateTime GetNextDueDate(
        int dueDay,
        DateTime today)
    {
        int thisMonthDays =
            DateTime.DaysInMonth(
                today.Year,
                today.Month);



        int safeDueDay =
            Math.Clamp(
                dueDay,
                1,
                thisMonthDays);



        DateTime thisMonthDueDate =
            new DateTime(
                today.Year,
                today.Month,
                safeDueDay);



        if (thisMonthDueDate.Date >=
            today.Date)
        {
            return
                thisMonthDueDate;
        }



        DateTime nextMonth =
            today.AddMonths(1);



        int nextMonthDays =
            DateTime.DaysInMonth(
                nextMonth.Year,
                nextMonth.Month);



        int nextMonthDueDay =
            Math.Clamp(
                dueDay,
                1,
                nextMonthDays);



        return
            new DateTime(
                nextMonth.Year,
                nextMonth.Month,
                nextMonthDueDay);
    }



    // ==========================================
    // POCKETAI INSIGHT
    // ==========================================

    private void UpdatePocketAiInsight(
        FinancialSnapshot snapshot,
        AccountBalance? accountBalance,
        SavingsGoal? savingsGoal,
        List<Expense> currentMonthExpenses,
        DateTime today)
    {
        // ======================================
        // NO ACCOUNT INFORMATION
        // ======================================

        if (accountBalance == null)
        {
            PocketAIInsightLabel.Text =
                "Add your current account balances so PocketAI can calculate how much money is actually available now.";


            return;
        }



        // ======================================
        // OBLIGATION SHORTFALL
        // ======================================

        if (snapshot.ObligationShortfall > 0)
        {
            PocketAIInsightLabel.Text =
                $"Your monthly plan is {snapshot.ObligationShortfall:C} short after " +
                $"spending, nills, required savings, and accepted extra savings. " +
                $"Review your plan before making extra purchases.";


            return;
        }



        // ======================================
        // LOW DATA CONFIDENCE
        // ======================================

        if (snapshot.DataConfidence.Equals(
                "Low",
                StringComparison.OrdinalIgnoreCase))
        {
            if (snapshot.RequiredSavingsThisMonth > 0)
            {
                PocketAIInsightLabel.Text =
                    $"{snapshot.RequiredSavingsThisMonth:C} is currently protected for your savings goals, " +
                    $"leaving an estimated {snapshot.SafeToSpendTotal:C} Safe to Spend based on the financial information entered so far.";
            }
            else
            {
                PocketAIInsightLabel.Text =
                    $"You currently have an estimated {snapshot.SafeToSpendTotal:C} Safe to Spend based on the financial information entered so far.";
            }


            return;
        }



        // ======================================
        // OVER BUDGET
        // ======================================

        if (snapshot.OverBudgetCount > 0)
        {
            string categoryText =
                snapshot.OverBudgetCount == 1
                    ? "category is"
                    : "categories are";


            PocketAIInsightLabel.Text =
                $"{snapshot.OverBudgetCount} budget {categoryText} currently over budget. " +
                "Review those limits before adding discretionary spending.";


            return;
        }



        // ======================================
        // ACTIVE SAVINGS GOAL
        // ======================================

        if (savingsGoal != null &&
            savingsGoal.CurrentAmount <
            savingsGoal.TargetAmount)
        {
            double weeklyNeeded =
                CalculateWeeklySavingsNeeded(
                    savingsGoal,
                    today);



            if (weeklyNeeded > 0)
            {
                PocketAIInsightLabel.Text =
                    $"You're working toward {savingsGoal.Name}. " +
                    $"About {weeklyNeeded:C} per week keeps that goal on its current schedule. " +
                    $"Your Safe to Spend already protects required savings.";


                return;
            }
        }



        // ======================================
        // BIGGEST SPENDING CATEGORY
        // ======================================

        var biggestCategory =
            currentMonthExpenses
                .Where(
                    expense =>
                        !string.IsNullOrWhiteSpace(
                            expense.Category))
                .GroupBy(
                    expense =>
                        expense.Category)
                .Select(
                    group =>
                        new
                        {
                            Category =
                                group.Key,

                            Amount =
                                group.Sum(
                                    expense =>
                                        Math.Max(
                                            expense.Amount,
                                            0))
                        })
                .OrderByDescending(
                    item =>
                        item.Amount)
                .FirstOrDefault();



        if (biggestCategory != null &&
            biggestCategory.Amount > 0)
        {
            PocketAIInsightLabel.Text =
                $"{biggestCategory.Category} is your largest spending category this month at " +
                $"{biggestCategory.Amount:C}. " +
                $"You currently have {snapshot.SafeToSpendTotal:C} total Safe to Spend.";


            return;
        }



        // ======================================
        // HEALTHY SAFE TO SPEND
        // ======================================

        if (snapshot.SafeToSpendTotal > 0)
        {
            PocketAIInsightLabel.Text =
                $"You currently have {snapshot.SafeToSpendTotal:C} total Safe to Spend, " +
                $"including about {snapshot.SafeToSpendToday:C} for today.";


            return;
        }



        // ======================================
        // FALLBACK
        // ======================================

        PocketAIInsightLabel.Text =
            "Keep adding real financial activity so PocketAI can make its guidance more precise.";
    }



    // ==========================================
    // HOME PAGE NAVIGATION
    // ==========================================


    // ==========================================
    // ADD EXPENSE
    // ==========================================

    private async void AddExpenseClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//Transactions");
    }



    // ==========================================
    // FINANCIAL HEALTH / ANALYTICS
    // ==========================================

    private async void ViewAnalyticsClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//Analytics");
    }



    // ==========================================
    // POCKETAI
    // ==========================================

    private async void AskPocketAIClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//PocketAI");
    }



    // ==========================================
    // SAVINGS
    // ==========================================

    private async void ViewSavingsClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//Savings");
    }



    // ==========================================
    // BUDGET
    // ==========================================

    private async void ViewBudgetClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//Budget");
    }



    // ==========================================
    // BILLS
    // ==========================================

    private async void ViewBillsClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//Bills");
    }
}