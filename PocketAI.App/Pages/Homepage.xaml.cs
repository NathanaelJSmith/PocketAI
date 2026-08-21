
namespace PocketAI.App.Pages;

public partial class HomePage : ContentPage
{
    private readonly DataBaseManager dataBaseManager;
    private readonly AnalyticsService analyticsService;

    public HomePage()
    {
        InitializeComponent();

        // Stores PocketAI's database in the app's data folder
        string databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "pocketai.db");

        // Connects PocketAI to its SQLite database
        dataBaseManager = new DataBaseManager(databasePath);

        // Handles PocketAI's financial calculations
        analyticsService = new AnalyticsService();

        // Makes sure all database tables exist
        dataBaseManager.CreateTables();
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Reloads Home every time the user returns to it
        LoadFinancialData();
    }


    private void LoadFinancialData()
    {
        // ==========================================
        // LOAD DATABASE INFORMATION
        // ==========================================

        List<Expense> expenses =
            dataBaseManager.GetAllExpenses();

        Income? income =
            dataBaseManager.GetIncome();

        AccountBalance? accountBalance =
            dataBaseManager.GetAccountBalance();

        // This is the PRIMARY savings goal.
        // Home uses this one for the savings card.
        SavingsGoal? savingsGoal =
            dataBaseManager.GetSavingsGoal();


        // This loads EVERY savings goal.
        // Safe to Spend will use all of these.
        List<SavingsGoal> savingsGoals =
            dataBaseManager.GetSavingsGoals();

        List<BudgetLimit> budgetLimits =
            dataBaseManager.GetBudgetLimits();

        List<RecurringExpenses> recurringExpenses =
            dataBaseManager.GetRecuringExpenses();


        // ==========================================
        // BUILD FINANCIAL SUMMARY
        // ==========================================

        FinancialSummary summary =
            analyticsService.BuildFinancialSummary(
                expenses,
                income,
                accountBalance,
                savingsGoal,
                budgetLimits,
                recurringExpenses);


        // ==========================================
        // DATE INFORMATION
        // ==========================================

        DateTime today = DateTime.Today;

        int daysInMonth =
            DateTime.DaysInMonth(
                today.Year,
                today.Month);

        int daysPassed =
            today.Day;

        int daysLeftInMonth =
            daysInMonth - today.Day + 1;


        // ==========================================
        // SAVINGS NEEDED THIS MONTH
        // ==========================================

        double savingsNeededThisMonth = 0;

        // Go through every savings goal
        foreach (SavingsGoal goal in savingsGoals)
        {
            // How much money is still needed
            // to complete this specific goal
            double amountRemaining =
                Math.Max(
                    goal.TargetAmount -
                    goal.CurrentAmount,
                    0);


            // How many days remain until
            // this specific goal's deadline
            double daysUntilDeadline =
                (goal.DeadLine.Date -
                DateTime.Today).TotalDays;


            // If the goal is already complete,
            // it should not reduce Safe to Spend
            if (amountRemaining <= 0)
            {
                continue;
            }


            // Calculate how much this one goal
            // needs during the current month
            double goalSavingsNeededThisMonth =
                analyticsService.GetSavingsNeededThisMonth(
                    amountRemaining,
                    daysUntilDeadline,
                    daysLeftInMonth);


            // Add this goal's requirement to
            // the total savings commitment
            savingsNeededThisMonth +=
                goalSavingsNeededThisMonth;
        }


        // ==========================================
        // SAFE TO SPEND
        // ==========================================

        double safeToSpend =
            analyticsService.GetSafeToSpend(
                summary.MoneyLeft,
                savingsNeededThisMonth);

        double dailySafeToSpend =
            analyticsService.GetDailySafeToSpend(
                safeToSpend,
                daysLeftInMonth);

        double weeksLeft =
            daysLeftInMonth / 7.0;

        double weeklySafeToSpend =
            analyticsService.GetWeeklySafeToSpend(
                safeToSpend,
                weeksLeft);


        // ==========================================
        // CASH FLOW FORECAST
        // ==========================================

        double averageDailySpending =
            analyticsService.GetAverageDailySpending(
                summary.CurrentMonthSpent,
                daysPassed);

        double projectedAdditionalSpending =
            analyticsService.GetProjectedAdditionalSpending(
                averageDailySpending,
                daysLeftInMonth);

        double projectedEndOfMonthMoney =
            analyticsService.GetProjectedEndOfMonthMoney(
                summary.MoneyLeft,
                projectedAdditionalSpending);


        // ==========================================
        // FINANCIAL HEALTH
        // ==========================================

        int financialHealthScore =
            analyticsService.GetFinancialHealthScore(
                summary,
                projectedEndOfMonthMoney,
                safeToSpend);

        string financialHealthStatus =
            analyticsService.GetFinancialHealthStatus(
                financialHealthScore);


        // ==========================================
        // UPDATE SAFE TO SPEND CARD
        // ==========================================

        DailySafeToSpendLabel.Text =
            dailySafeToSpend.ToString("C");

        WeeklySafeToSpendLabel.Text =
            $"{weeklySafeToSpend:C} safe this week";

        if (safeToSpend >= 0)
        {
            SafeToSpendStatusLabel.Text =
                "✓ You're on track";

            SafeToSpendStatusLabel.TextColor =
                Color.FromArgb("#15803D");
        }
        else
        {
            SafeToSpendStatusLabel.Text =
                $"⚠ {Math.Abs(safeToSpend):C} short";

            SafeToSpendStatusLabel.TextColor =
                Color.FromArgb("#B91C1C");
        }


        // ==========================================
        // UPDATE FINANCIAL HEALTH CARD
        // ==========================================

        FinancialHealthScoreLabel.Text =
            $"{financialHealthScore} / 100";

        FinancialHealthStatusLabel.Text =
            financialHealthStatus;


        // ==========================================
        // UPDATE YOUR MONEY CARDS
        // ==========================================

        TotalBalanceLabel.Text =
            summary.TotalAccountBalance.ToString("C");

        MonthlyIncomeLabel.Text =
            summary.MonthlyIncome.ToString("C");

        MonthlySpentLabel.Text =
            summary.CurrentMonthSpent.ToString("C");

        MoneyLeftLabel.Text =
            summary.MoneyLeft.ToString("C");

            // ==========================================
            // UPDATE SAVINGS GOAL CARD
            // ==========================================

    if (savingsGoal != null)
    {
        SavingsGoalNameLabel.Text =
            summary.SavingsGoalName;

        SavingsProgressAmountLabel.Text =
            $"{summary.CurrentSavedAmount:C} / {summary.SavingsTargetAmount:C}";

        SavingsProgressPercentLabel.Text =
            $"{summary.SavingsProgressPercentage:F0}%";

        // ProgressBar uses 0.0 - 1.0 instead of 0 - 100
        SavingsProgressBar.Progress =
            summary.SavingsProgressPercentage / 100.0;

        SavingsRemainingLabel.Text =
            $"{summary.SavingsAmountRemaining:C} remaining";

        WeeklySavingsNeededLabel.Text =
            $"{summary.WeeklySavingsNeeded:C}/week needed";

        if (summary.SavingsAmountRemaining <= 0)
        {
            SavingsStatusLabel.Text =
                "✓ Goal complete";

            SavingsStatusLabel.TextColor =
                Color.FromArgb("#15803D");
        }
        else if (summary.DaysLeft <= 0)
        {
            SavingsStatusLabel.Text =
                "⚠ Target date reached";

            SavingsStatusLabel.TextColor =
                Color.FromArgb("#B91C1C");
        }
        else
        {
            SavingsStatusLabel.Text =
                $"{summary.DaysLeft} days remaining";

            SavingsStatusLabel.TextColor =
                Color.FromArgb("#15803D");
        }
    }
        else
        {
        SavingsGoalNameLabel.Text =
            "No active goal";

        SavingsProgressAmountLabel.Text =
            "$0.00 / $0.00";

        SavingsProgressPercentLabel.Text =
            "0%";

        SavingsProgressBar.Progress = 0;

        SavingsRemainingLabel.Text =
            "$0.00 remaining";

        WeeklySavingsNeededLabel.Text =
            "$0.00/week needed";

        SavingsStatusLabel.Text =
            "No savings goal";

        SavingsStatusLabel.TextColor =
            Color.FromArgb("#6B7280");
        }

        // ==========================================
        // UPDATE BUDGET SNAPSHOT
        // ==========================================
        BudgetSnapshotContainer.Children.Clear();

        BudgetSnapshotContainer.Children.Clear();

if (budgetLimits.Count > 0)
{
    BudgetEmptyLabel.IsVisible = false;

    // Only expenses from the current month
    List<Expense> currentMonthExpenses =
        expenses
            .Where(expense =>
                expense.Date.Year == today.Year &&
                expense.Date.Month == today.Month)
            .ToList();

    // Shows up to 3 budget categories
    foreach (BudgetLimit budget in budgetLimits.Take(3))
    {
        double spent =
            analyticsService.GetCategoryTotal(
                currentMonthExpenses,
                budget.Category);

        double remaining =
            budget.LimitAmount - spent;

        double progress = 0;

        if (budget.LimitAmount > 0)
        {
            progress =
                spent / budget.LimitAmount;
        }

        // ProgressBar should stay between 0 and 1
        double displayedProgress =
            Math.Clamp(progress, 0, 1);


        // Category name and amount
        Grid topRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };

        Label categoryLabel = new Label
        {
            Text = budget.Category,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#111827")
        };

        Label amountLabel = new Label
        {
            Text = $"{spent:C} / {budget.LimitAmount:C}",
            FontSize = 13,
            TextColor = Color.FromArgb("#6B7280")
        };

        Grid.SetColumn(categoryLabel, 0);
        Grid.SetColumn(amountLabel, 1);

        topRow.Children.Add(categoryLabel);
        topRow.Children.Add(amountLabel);


        // Budget progress bar
        ProgressBar progressBar = new ProgressBar
        {
            Progress = displayedProgress,
            ProgressColor =
                remaining < 0
                    ? Color.FromArgb("#B91C1C")
                    : Color.FromArgb("#7C3AED"),

            BackgroundColor =
                Color.FromArgb("#E5E7EB"),

            HeightRequest = 8
        };


        // Remaining / over-budget message
        Label remainingLabel = new Label
        {
            FontSize = 12
        };

        if (remaining >= 0)
        {
            remainingLabel.Text =
                $"{remaining:C} left";

            remainingLabel.TextColor =
                Color.FromArgb("#6B7280");
        }
        else
        {
            remainingLabel.Text =
                $"{Math.Abs(remaining):C} OVER BUDGET";

            remainingLabel.TextColor =
                Color.FromArgb("#B91C1C");

            remainingLabel.FontAttributes =
                FontAttributes.Bold;
        }


        VerticalStackLayout budgetRow =
            new VerticalStackLayout
            {
                Spacing = 6
            };

        budgetRow.Children.Add(topRow);
        budgetRow.Children.Add(progressBar);
        budgetRow.Children.Add(remainingLabel);

        BudgetSnapshotContainer.Children.Add(
            budgetRow);
    }
    }
    else
    {
        BudgetEmptyLabel.IsVisible = true;

        BudgetEmptyLabel.Text =
        "No budget information yet.";
    }

    // ==========================================
// UPDATE UPCOMING BILLS
// ==========================================

// Clears the old bill rows whenever Home reloads
UpcomingBillsContainer.Children.Clear();

// Only use active recurring bills
List<RecurringExpenses> activeBills =
    recurringExpenses
        .Where(bill => bill.IsActive)
        .OrderBy(bill =>
            analyticsService.GetDaysUntilDue(bill.DueDay))
        .Take(3)
        .ToList();

    if (activeBills.Count > 0)
    {
    BillsEmptyLabel.IsVisible = false;

    foreach (RecurringExpenses bill in activeBills)
    {
        int daysUntilDue =
            analyticsService.GetDaysUntilDue(
                bill.DueDay);

        // Figures out the actual upcoming due date
        DateTime dueDate;

        int thisMonthDueDay =
            Math.Min(
                bill.DueDay,
                DateTime.DaysInMonth(
                    today.Year,
                    today.Month));

        DateTime thisMonthDueDate =
            new DateTime(
                today.Year,
                today.Month,
                thisMonthDueDay);

        if (thisMonthDueDate.Date >= today.Date)
        {
            dueDate = thisMonthDueDate;
        }
        else
        {
            DateTime nextMonth =
                today.AddMonths(1);

            int nextMonthDueDay =
                Math.Min(
                    bill.DueDay,
                    DateTime.DaysInMonth(
                        nextMonth.Year,
                        nextMonth.Month));

            dueDate =
                new DateTime(
                    nextMonth.Year,
                    nextMonth.Month,
                    nextMonthDueDay);
        }


        // Main bill row
        Grid billRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },

            ColumnSpacing = 20
        };


        // Left side
        VerticalStackLayout billInfo =
            new VerticalStackLayout
            {
                Spacing = 3
            };

        Label nameLabel = new Label
        {
            Text = bill.Name,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#111827")
        };

        Label categoryLabel = new Label
        {
            Text = bill.Category,
            FontSize = 12,
            TextColor = Color.FromArgb("#6B7280")
        };

        Label dueLabel = new Label
        {
            Text =
                $"Due {dueDate:MMM d} • " +
                (daysUntilDue == 0
                    ? "Due today"
                    : daysUntilDue == 1
                        ? "Due tomorrow"
                        : $"Due in {daysUntilDue} days"),

            FontSize = 12,
            TextColor = Color.FromArgb("#6B7280")
        };

        billInfo.Children.Add(nameLabel);
        billInfo.Children.Add(categoryLabel);
        billInfo.Children.Add(dueLabel);


        // Right side — amount
        Label amountLabel = new Label
        {
            Text = bill.Amount.ToString("C"),
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#111827"),
            VerticalOptions = LayoutOptions.Center
        };

        Grid.SetColumn(billInfo, 0);
        Grid.SetColumn(amountLabel, 1);

        billRow.Children.Add(billInfo);
        billRow.Children.Add(amountLabel);

        UpcomingBillsContainer.Children.Add(
            billRow);
    }
    }
    else
    {
        BillsEmptyLabel.IsVisible = true;

        BillsEmptyLabel.Text =
        "No upcoming bills.";
    }

    // ==========================================
    // UPDATE POCKETAI INSIGHT
    // ==========================================

    if (income == null &&
        expenses.Count == 0 &&
        accountBalance == null)
    {
        PocketAIInsightLabel.Text =
            "Complete your financial setup so PocketAI can start analyzing your money.";
    }
    else if (safeToSpend < 0)
    {
        PocketAIInsightLabel.Text =
            $"Your current plan is {Math.Abs(safeToSpend):C} short. " +
            "Consider reducing spending or adjusting your savings plan.";
    }
    else if (summary.OverBudgetCount > 0)
    {
        string categoryText =
            summary.OverBudgetCount == 1
                ? "category is"
                : "categories are";

        PocketAIInsightLabel.Text =
            $"{summary.OverBudgetCount} budget {categoryText} currently over budget. " +
            "Review those categories before making extra purchases.";
    }
    else if (savingsGoal != null &&
            summary.SavingsAmountRemaining > 0)
    {
        PocketAIInsightLabel.Text =
            $"You're working toward {summary.SavingsGoalName}. " +
            $"Saving about {summary.WeeklySavingsNeeded:C} per week " +
            "will help keep your goal on track.";
    }
    else if (!string.IsNullOrWhiteSpace(
                summary.BiggestSpendingCategory) &&
            summary.BiggestCategoryAmount > 0)
    {
        PocketAIInsightLabel.Text =
            $"{summary.BiggestSpendingCategory} is your largest spending category " +
            $"this month at {summary.BiggestCategoryAmount:C}.";
    }
    else if (safeToSpend > 0)
    {
        PocketAIInsightLabel.Text =
            $"You're currently on track with approximately " +
            $"{dailySafeToSpend:C} available to safely spend today.";
    }
    else
    {
        PocketAIInsightLabel.Text =
            "Add more financial activity so PocketAI can give you a useful recommendation.";
    }
    }
}