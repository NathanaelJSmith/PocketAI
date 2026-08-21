using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace PocketAI.App.Pages;

public partial class AnalyticsPage : ContentPage
{
    private readonly DataBaseManager dataBaseManager;
    private readonly AnalyticsService analyticsService;


    public AnalyticsPage()
    {
        InitializeComponent();


        // Stores PocketAI's database inside
        // the MAUI application's data folder.
        string databasePath =
            Path.Combine(
                FileSystem.AppDataDirectory,
                "pocketai.db");


        // Connects this page to SQLite.
        dataBaseManager =
            new DataBaseManager(
                databasePath);


        // Handles PocketAI's financial calculations.
        analyticsService =
            new AnalyticsService();


        // Makes sure all database tables exist.
        dataBaseManager.CreateTables();
    }



    // ==========================================
    // PAGE APPEARS
    // ==========================================

    protected override void OnAppearing()
    {
        base.OnAppearing();


        // Reload analytics whenever the
        // user returns to this page.
        LoadAnalytics();
    }



    // ==========================================
    // LOAD ALL ANALYTICS DATA
    // ==========================================

    private void LoadAnalytics()
    {
        // Transactions
        List<Expense> expenses =
            dataBaseManager
                .GetAllExpenses();


        // Monthly income
        Income? income =
            dataBaseManager
                .GetIncome();


        // Checking, savings, and cash balances
        AccountBalance? accountBalance =
            dataBaseManager
                .GetAccountBalance();


        // Primary savings goal
        SavingsGoal? primarySavingsGoal =
            dataBaseManager
                .GetSavingsGoal();


        // Every savings goal
        List<SavingsGoal> savingsGoals =
            dataBaseManager
                .GetSavingsGoals();


        // Budget category limits
        List<BudgetLimit> budgets =
            dataBaseManager
                .GetBudgetLimits();


        // Recurring bills
        List<RecurringExpenses> recurringExpenses =
            dataBaseManager
                .GetRecuringExpenses();


        // Builds PocketAI's main financial summary.
        FinancialSummary summary =
            analyticsService
                .BuildFinancialSummary(
                    expenses,
                    income,
                    accountBalance,
                    primarySavingsGoal,
                    budgets,
                    recurringExpenses);


        // Load every Analytics section.
        LoadOverview(
            expenses,
            savingsGoals,
            summary);


        LoadSpending(
            expenses);


        LoadWeeklySpendingChart(
            expenses);


        LoadCashFlow(
            summary);


        LoadTrends(
            expenses);
    }



    // ==========================================
    // OVERVIEW TAB
    // ==========================================

    private void LoadOverview(
        List<Expense> expenses,
        List<SavingsGoal> savingsGoals,
        FinancialSummary summary)
    {
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


        int daysPassed =
            today.Day;



        // ======================================
        // SAVINGS NEEDED THIS MONTH
        // ACROSS ALL SAVINGS GOALS
        // ======================================

        double savingsNeededThisMonth =
            0;


        foreach (SavingsGoal goal
                 in savingsGoals)
        {
            double remaining =
                Math.Max(
                    goal.TargetAmount -
                    goal.CurrentAmount,
                    0);


            double daysUntilDeadline =
                (goal.DeadLine.Date -
                 today.Date).TotalDays;


            // Completed goals should not
            // reduce Safe to Spend.
            if (remaining <= 0)
            {
                continue;
            }


            double goalSavingsNeeded =
                analyticsService
                    .GetSavingsNeededThisMonth(
                        remaining,
                        daysUntilDeadline,
                        daysLeftInMonth);


            savingsNeededThisMonth +=
                goalSavingsNeeded;
        }



        // ======================================
        // SAFE TO SPEND
        // ======================================

        double safeToSpend =
            analyticsService
                .GetSafeToSpend(
                    summary.MoneyLeft,
                    savingsNeededThisMonth);


        double dailySafe =
            analyticsService
                .GetDailySafeToSpend(
                    safeToSpend,
                    daysLeftInMonth);


        double weeksLeft =
            daysLeftInMonth /
            7.0;


        double weeklySafe =
            analyticsService
                .GetWeeklySafeToSpend(
                    safeToSpend,
                    weeksLeft);



        // ======================================
        // END OF MONTH PROJECTION
        // ======================================

        double averageDaily =
            analyticsService
                .GetAverageDailySpending(
                    summary.CurrentMonthSpent,
                    daysPassed);


        double projectedAdditional =
            analyticsService
                .GetProjectedAdditionalSpending(
                    averageDaily,
                    daysLeftInMonth);


        double projectedMoney =
            analyticsService
                .GetProjectedEndOfMonthMoney(
                    summary.MoneyLeft,
                    projectedAdditional);



        // ======================================
        // FINANCIAL HEALTH
        // ======================================

        int healthScore =
            analyticsService
                .GetFinancialHealthScore(
                    summary,
                    projectedMoney,
                    safeToSpend);


        string healthStatus =
            analyticsService
                .GetFinancialHealthStatus(
                    healthScore);



        // ======================================
        // UPDATE OVERVIEW LABELS
        // ======================================

        OverviewIncomeLabel.Text =
            summary.MonthlyIncome
                .ToString("C");


        OverviewSpentLabel.Text =
            summary.CurrentMonthSpent
                .ToString("C");


        OverviewRecurringLabel.Text =
            summary.MonthlyRecurringExpenses
                .ToString("C");


        OverviewMoneyLeftLabel.Text =
            summary.MoneyLeft
                .ToString("C");


        HealthScoreLabel.Text =
            $"{healthScore} / 100";


        HealthStatusLabel.Text =
            healthStatus;


        HealthProgressBar.Progress =
            healthScore /
            100.0;


        SafeToSpendLabel.Text =
            safeToSpend
                .ToString("C");


        DailySafeLabel.Text =
            $"{dailySafe:C} per day";


        WeeklySafeLabel.Text =
            $"{weeklySafe:C} per week";



        // ======================================
        // POCKETAI SUMMARY
        // ======================================

        if (summary.MonthlyIncome <= 0 &&
            expenses.Count == 0)
        {
            OverviewInsightLabel.Text =
                "Add income and transactions so PocketAI can begin analyzing your finances.";
        }

        else if (safeToSpend < 0)
        {
            OverviewInsightLabel.Text =
                $"Your current financial plan is {Math.Abs(safeToSpend):C} short after accounting for spending, bills, and savings goals.";
        }

        else if (summary.OverBudgetCount > 0)
        {
            string categoryText =
                summary.OverBudgetCount == 1
                    ? "category is"
                    : "categories are";


            OverviewInsightLabel.Text =
                $"{summary.OverBudgetCount} budget {categoryText} currently over the limit. Review those categories before increasing discretionary spending.";
        }

        else
        {
            OverviewInsightLabel.Text =
                $"You currently have about {safeToSpend:C} available after your planned savings commitments.";
        }
    }



    // ==========================================
    // SPENDING TAB
    // ==========================================

    private void LoadSpending(
        List<Expense> expenses)
    {
        // Current month's transactions
        List<Expense> currentMonth =
            analyticsService
                .GetCurrentMonthExpense(
                    expenses);


        // Previous month's transactions
        List<Expense> lastMonth =
            analyticsService
                .GetLastMonthExpense(
                    expenses);


        double currentSpent =
            analyticsService
                .GetTotalSpent(
                    currentMonth);


        double lastSpent =
            analyticsService
                .GetTotalSpent(
                    lastMonth);




        // ======================================
        // MONTH COMPARISON
        // ======================================

        CurrentMonthSpentLabel.Text =
            currentSpent
                .ToString("C");


        LastMonthSpentLabel.Text =
            lastSpent
                .ToString("C");


        UpdateSpendingComparison(
        SpendingChangeLabel,
        currentSpent,
        lastSpent,
        "last month");



        // ======================================
        // CATEGORY DATA
        // ======================================

        List<CategorySpendingItem> categories =
            currentMonth
                .GroupBy(
                    expense =>
                        expense.Category)
                .Select(
                    group =>
                        new CategorySpendingItem
                        {
                            Category =
                                group.Key,

                            Amount =
                                group.Sum(
                                    expense =>
                                        expense.Amount)
                        })
                .OrderByDescending(
                    item =>
                        item.Amount)
                .ToList();



        // ======================================
        // TOTAL CATEGORY SPENDING
        // ======================================

        double total =
            categories.Sum(
                item =>
                    item.Amount);


        // Shows the total in the
        // center of the donut.
        CategoryDonutTotalLabel.Text =
            total.ToString("C");


        CategoryDonutCenter.IsVisible =
            total > 0;



        // Gives every category the monthly
        // total so percentages can be calculated.
        foreach (CategorySpendingItem item
                 in categories)
        {
            item.TotalSpending =
                total;
        }



        // ======================================
        // CATEGORY DONUT GRAPH
        // ======================================

        List<ISeries> categorySeries =
            new List<ISeries>();


        foreach (CategorySpendingItem category
                 in categories)
        {
            categorySeries.Add(
                new PieSeries<double>
                {
                    Name =
                        category.Category,


                    Values =
                        new double[]
                        {
                            category.Amount
                        },


                    // Creates the hole in
                    // the center of the pie.
                    InnerRadius =
                        70,


                    // Formats the hover tooltip
                    // as actual money.
                    ToolTipLabelFormatter =
                        point =>
                            point.Model
                                .ToString("C2")
                });
        }


        CategoryDonutChart.Series =
            categorySeries;


        CategoryDonutChart.IsVisible =
            categories.Count > 0;



        // ======================================
        // CATEGORY BREAKDOWN LIST
        // ======================================

        BindableLayout.SetItemsSource(
            CategorySpendingContainer,
            categories);


        CategoryEmptyLabel.IsVisible =
            categories.Count == 0;
    }



    // ==========================================
    // THIS WEEK VS LAST WEEK
    // ==========================================

    private void LoadWeeklySpendingChart(
        List<Expense> expenses)
    {
        List<Expense> currentWeek =
            analyticsService
                .GetCurrentWeekExpenses(
                    expenses);


        List<Expense> lastWeek =
            analyticsService
                .GetLastWeekExpenses(
                    expenses);



        // Monday through Sunday.
        DayOfWeek[] days =
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        };


        double[] thisWeekValues =
            new double[7];


        double[] lastWeekValues =
            new double[7];



        // ======================================
        // CALCULATE EACH DAY
        // ======================================

        for (int i = 0;
             i < days.Length;
             i++)
        {
            DayOfWeek day =
                days[i];


            thisWeekValues[i] =
                currentWeek
                    .Where(
                        expense =>
                            expense.Date.DayOfWeek ==
                            day)
                    .Sum(
                        expense =>
                            expense.Amount);


            lastWeekValues[i] =
                lastWeek
                    .Where(
                        expense =>
                            expense.Date.DayOfWeek ==
                            day)
                    .Sum(
                        expense =>
                            expense.Amount);
        }



        // ======================================
        // WEEK TOTALS
        // ======================================

        double thisWeekTotal =
            thisWeekValues.Sum();


        double lastWeekTotal =
            lastWeekValues.Sum();


        ThisWeekTotalLabel.Text =
            thisWeekTotal
                .ToString("C");


        LastWeekTotalLabel.Text =
            lastWeekTotal
                .ToString("C");



        // ======================================
        // WEEKLY CHANGE
        // ======================================

        // Displays the weekly comparison
        // without showing confusing giant percentages.
        UpdateSpendingComparison(
            WeeklyChangeLabel,
            thisWeekTotal,
            lastWeekTotal,
            "last week");



        // ======================================
        // WEEKLY BAR GRAPH
        // ======================================

        WeekComparisonChart.Series =
            new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name =
                        "This Week",


                    Values =
                        thisWeekValues,


                    // Currency tooltip
                    YToolTipLabelFormatter =
                        point =>
                            point.Model
                                .ToString("C2")
                },


                new ColumnSeries<double>
                {
                    Name =
                        "Last Week",


                    Values =
                        lastWeekValues,


                    // Currency tooltip
                    YToolTipLabelFormatter =
                        point =>
                            point.Model
                                .ToString("C2")
                }
            };



        // ======================================
        // X AXIS
        // ======================================

        WeekComparisonChart.XAxes =
            new Axis[]
            {
                new Axis
                {
                    Labels =
                        new[]
                        {
                            "Mon",
                            "Tue",
                            "Wed",
                            "Thu",
                            "Fri",
                            "Sat",
                            "Sun"
                        }
                }
            };



        // ======================================
        // Y AXIS
        // ======================================

        WeekComparisonChart.YAxes =
            new Axis[]
            {
                new Axis
                {
                    MinLimit =
                        0,


                    Labeler =
                        value =>
                            value.ToString(
                                "C0")
                }
            };
    }



    // ==========================================
    // CASH FLOW TAB
    // ==========================================

    private void LoadCashFlow(
        FinancialSummary summary)
    {
        double income =
            summary.MonthlyIncome;


        double spending =
            summary.CurrentMonthSpent;


        double bills =
            summary.MonthlyRecurringExpenses;


        double remaining =
            summary.MoneyLeft;



        // ======================================
        // CASH FLOW GRAPH
        // ======================================

        CashFlowChart.Series =
            new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name =
                        "Amount",


                    Values =
                        new double[]
                        {
                            income,
                            spending,
                            bills,
                            remaining
                        },


                    // Currency tooltip
                    YToolTipLabelFormatter =
                        point =>
                            point.Model
                                .ToString("C2")
                }
            };


        CashFlowChart.XAxes =
            new Axis[]
            {
                new Axis
                {
                    Labels =
                        new[]
                        {
                            "Income",
                            "Spending",
                            "Bills",
                            "Remaining"
                        }
                }
            };


        CashFlowChart.YAxes =
            new Axis[]
            {
                new Axis
                {
                    Labeler =
                        value =>
                            value.ToString(
                                "C0")
                }
            };



        // ======================================
        // CASH FLOW LABELS
        // ======================================

        CashFlowIncomeLabel.Text =
            income.ToString("C");


        CashFlowSpendingLabel.Text =
            $"-{spending:C}";


        CashFlowBillsLabel.Text =
            $"-{bills:C}";


        CashFlowRemainingLabel.Text =
            remaining.ToString("C");



        // ======================================
        // INCOME USAGE
        // ======================================

        double committed =
            spending +
            bills;


        double usagePercent =
            0;


        if (income > 0)
        {
            usagePercent =
                committed /
                income;
        }


        IncomeUsedProgressBar.Progress =
            Math.Clamp(
                usagePercent,
                0,
                1);


        IncomeUsedLabel.Text =
            $"{usagePercent * 100:F0}% of monthly income committed";



        // ======================================
        // END OF MONTH PROJECTION
        // ======================================

        DateTime today =
            DateTime.Today;


        int daysInMonth =
            DateTime.DaysInMonth(
                today.Year,
                today.Month);


        int daysLeft =
            daysInMonth -
            today.Day +
            1;


        double averageDaily =
            analyticsService
                .GetAverageDailySpending(
                    spending,
                    today.Day);


        double projectedAdditional =
            analyticsService
                .GetProjectedAdditionalSpending(
                    averageDaily,
                    daysLeft);


        double projectedMoney =
            analyticsService
                .GetProjectedEndOfMonthMoney(
                    remaining,
                    projectedAdditional);


        ProjectedMoneyLabel.Text =
            projectedMoney
                .ToString("C");



        if (spending <= 0)
        {
            ProjectionMessageLabel.Text =
                "There isn't enough spending data yet to create a useful projection.";
        }

        else if (projectedMoney < 0)
        {
            ProjectionMessageLabel.Text =
                $"At your current spending pace, you may finish the month about {Math.Abs(projectedMoney):C} short.";
        }

        else
        {
            ProjectionMessageLabel.Text =
                $"At your current spending pace, you may finish the month with about {projectedMoney:C} remaining.";
        }
    }



    // ==========================================
    // SIX-MONTH SPENDING TREND
    // ==========================================

    private void LoadTrends(
        List<Expense> expenses)
    {
        DateTime currentMonth =
            new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);


        string[] monthLabels =
            new string[6];


        double[] monthlyValues =
            new double[6];



        // ======================================
        // BUILD SIX MONTHS
        // ======================================

        for (int i = 0;
             i < 6;
             i++)
        {
            DateTime month =
                currentMonth
                    .AddMonths(
                        i - 5);


            monthLabels[i] =
                month.ToString(
                    "MMM");


            monthlyValues[i] =
                expenses
                    .Where(
                        expense =>
                            expense.Date.Year ==
                            month.Year
                            &&
                            expense.Date.Month ==
                            month.Month)
                    .Sum(
                        expense =>
                            expense.Amount);
        }



        // ======================================
        // SIX-MONTH LINE GRAPH
        // ======================================

        SixMonthTrendChart.Series =
            new ISeries[]
            {
                new LineSeries<double>
                {
                    Name =
                        "Monthly Spending",


                    Values =
                        monthlyValues,


                    // Makes each month's
                    // point visible.
                    GeometrySize =
                        10,


                    // Gives the graph a
                    // slight curve.
                    LineSmoothness =
                        0.4,


                    // Prevents the graph from
                    // filling underneath the line.
                    Fill =
                        null,


                    // Currency tooltip
                    YToolTipLabelFormatter =
                        point =>
                            point.Model
                                .ToString("C2")
                }
            };



        // ======================================
        // X AXIS
        // ======================================

        SixMonthTrendChart.XAxes =
            new Axis[]
            {
                new Axis
                {
                    Labels =
                        monthLabels
                }
            };



        // ======================================
        // Y AXIS
        // ======================================

        SixMonthTrendChart.YAxes =
            new Axis[]
            {
                new Axis
                {
                    MinLimit =
                        0,


                    Labeler =
                        value =>
                            value.ToString(
                                "C0")
                }
            };



        // ======================================
        // SIX MONTH AVERAGE
        // ======================================

        double average =
            monthlyValues
                .Average();


        SixMonthAverageLabel.Text =
            average
                .ToString("C");



        // ======================================
        // CURRENT VS PREVIOUS MONTH
        // ======================================

        double previousMonth =
            monthlyValues[4];


        double currentMonthAmount =
            monthlyValues[5];


        if (previousMonth <= 0 &&
            currentMonthAmount <= 0)
        {
            TrendDirectionLabel.Text =
                "Not enough data";


            TrendDirectionLabel.TextColor =
                Color.FromArgb(
                    "#6B7280");
        }

        else if (currentMonthAmount >
                 previousMonth)
        {
            double difference =
                currentMonthAmount -
                previousMonth;


            TrendDirectionLabel.Text =
                $"↑ {difference:C} more";


            TrendDirectionLabel.TextColor =
                Color.FromArgb(
                    "#B91C1C");
        }

        else if (currentMonthAmount <
                 previousMonth)
        {
            double difference =
                previousMonth -
                currentMonthAmount;


            TrendDirectionLabel.Text =
                $"↓ {difference:C} less";


            TrendDirectionLabel.TextColor =
                Color.FromArgb(
                    "#15803D");
        }

        else
        {
            TrendDirectionLabel.Text =
                "→ Spending steady";


            TrendDirectionLabel.TextColor =
                Color.FromArgb(
                    "#6B7280");
        }
    }

    // ==========================================
    // SPENDING COMPARISON DISPLAY
    // ==========================================

    private void UpdateSpendingComparison(
        Label label,
        double currentAmount,
        double previousAmount,
        string comparisonPeriod)
    {
        // ======================================
        // NO SPENDING IN EITHER PERIOD
        // ======================================

        if (currentAmount <= 0 &&
            previousAmount <= 0)
        {
            label.Text =
                "No change";


            label.TextColor =
                Color.FromArgb(
                    "#6B7280");


            return;
        }



        // ======================================
        // NO PREVIOUS DATA
        // ======================================

        if (previousAmount <= 0)
        {
            label.Text =
                $"New spending\nNo {comparisonPeriod} data";


            label.TextColor =
                Color.FromArgb(
                    "#6B7280");


            return;
        }



        // ======================================
        // CALCULATE CHANGE
        // ======================================

        double percentageChange =
            analyticsService
                .GetSpendingPercentageChange(
                    currentAmount,
                    previousAmount);


        double dollarDifference =
            currentAmount -
            previousAmount;



        // ======================================
        // VERY LARGE INCREASE
        // ======================================

        // Giant percentages such as +1050%
        // are mathematically correct but not
        // very useful to the user.
        //
        // Instead, show the dollar increase
        // and how many times larger it is.
        if (percentageChange >= 200)
        {
            double multiplier =
                currentAmount /
                previousAmount;


            label.Text =
                $"Up {dollarDifference:C}\n{multiplier:0.0}× {comparisonPeriod}";


            label.TextColor =
                Color.FromArgb(
                    "#B91C1C");


            return;
        }



        // ======================================
        // NORMAL INCREASE
        // ======================================

        if (percentageChange > 0)
        {
            label.Text =
                $"+{percentageChange:0.0}%\nvs {comparisonPeriod}";


            label.TextColor =
                Color.FromArgb(
                    "#B91C1C");


            return;
        }



        // ======================================
        // DECREASE
        // ======================================

        if (percentageChange < 0)
        {
            label.Text =
                $"{percentageChange:0.0}%\nvs {comparisonPeriod}";


            label.TextColor =
                Color.FromArgb(
                    "#15803D");


            return;
        }



        // ======================================
        // SAME AMOUNT
        // ======================================

        label.Text =
            $"0%\nvs {comparisonPeriod}";


        label.TextColor =
            Color.FromArgb(
                "#6B7280");
    }

    // ==========================================
    // TAB BUTTONS
    // ==========================================

    private void OverviewTabClicked(
        object? sender,
        EventArgs e)
    {
        ShowTab(
            "Overview");
    }


    private void SpendingTabClicked(
        object? sender,
        EventArgs e)
    {
        ShowTab(
            "Spending");
    }


    private void CashFlowTabClicked(
        object? sender,
        EventArgs e)
    {
        ShowTab(
            "CashFlow");
    }


    private void TrendsTabClicked(
        object? sender,
        EventArgs e)
    {
        ShowTab(
            "Trends");
    }



    // ==========================================
    // SHOW SELECTED TAB
    // ==========================================

    private void ShowTab(
        string tab)
    {
        OverviewSection.IsVisible =
            tab ==
            "Overview";


        SpendingSection.IsVisible =
            tab ==
            "Spending";


        CashFlowSection.IsVisible =
            tab ==
            "CashFlow";


        TrendsSection.IsVisible =
            tab ==
            "Trends";


        SetTabButtonStyle(
            OverviewTabButton,
            tab ==
            "Overview");


        SetTabButtonStyle(
            SpendingTabButton,
            tab ==
            "Spending");


        SetTabButtonStyle(
            CashFlowTabButton,
            tab ==
            "CashFlow");


        SetTabButtonStyle(
            TrendsTabButton,
            tab ==
            "Trends");
    }



    // ==========================================
    // TAB BUTTON STYLE
    // ==========================================

    private void SetTabButtonStyle(
        Button button,
        bool selected)
    {
        if (selected)
        {
            button.BackgroundColor =
                Color.FromArgb(
                    "#7C3AED");


            button.TextColor =
                Colors.White;
        }

        else
        {
            button.BackgroundColor =
                Colors.Transparent;


            button.TextColor =
                Color.FromArgb(
                    "#6B7280");
        }
    }



    // ==========================================
    // CATEGORY DISPLAY MODEL
    // ==========================================

    public class CategorySpendingItem
    {
        public string Category
        {
            get;
            set;
        } = "";


        public double Amount
        {
            get;
            set;
        }


        public double TotalSpending
        {
            get;
            set;
        }



        // Dollar amount shown beside category.
        public string AmountText =>
            Amount.ToString(
                "C");



        // Percentage shown under category.
        public string PercentText
        {
            get
            {
                if (TotalSpending <= 0)
                {
                    return
                        "0% of spending";
                }


                double percentage =
                    Amount /
                    TotalSpending *
                    100;


                return
                    $"{percentage:F0}% of spending";
            }
        }



        // ProgressBar requires a number
        // between 0 and 1.
        public double Progress
        {
            get
            {
                if (TotalSpending <= 0)
                {
                    return 0;
                }


                return Math.Clamp(
                    Amount /
                    TotalSpending,
                    0,
                    1);
            }
        }
    }
}