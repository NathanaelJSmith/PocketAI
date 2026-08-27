using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

using SkiaSharp;


namespace PocketAI.App.Pages;

public partial class AnalyticsPage : ContentPage
{
    // ==========================================
    // DATABASE
    // ==========================================

    private readonly DataBaseManager
        dataBaseManager;


    // ==========================================
    // HISTORICAL ANALYTICS
    // ==========================================
    //
    // AnalyticsService remains useful for:
    //
    // - spending history
    // - category analysis
    // - week comparisons
    // - month comparisons
    // - trend calculations
    //
    // It is NO LONGER the source of truth for
    // Safe to Spend or Financial Health.
    // ==========================================

    private readonly AnalyticsService
        analyticsService;


    // ==========================================
    // CENTRAL FINANCIAL ENGINE
    // ==========================================

    private readonly FinancialSnapshotProvider
        financialSnapshotProvider;


    private string currentTab =
        "Overview";



    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public AnalyticsPage()
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


        analyticsService =
            new AnalyticsService();


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


        LoadAnalytics();


        ShowTab(
            currentTab);
    }



    // ==========================================
    // LOAD ANALYTICS
    // ==========================================

    private void LoadAnalytics()
    {
        // ======================================
        // ONE CENTRAL FINANCIAL SNAPSHOT
        // ======================================

        FinancialSnapshot snapshot =
            financialSnapshotProvider
                .GetSnapshot();



        // ======================================
        // TRANSACTION HISTORY
        // ======================================
        //
        // Raw transactions are still useful
        // for actual analytics and charts.
        // ======================================

        List<Expense> expenses =
            dataBaseManager
                .GetAllExpenses();



        // ======================================
        // LOAD SECTIONS
        // ======================================

        LoadOverview(
            snapshot);


        LoadSpending(
            expenses);


        LoadWeeklySpendingChart(
            expenses);


        LoadCashFlow(
            snapshot);


        LoadTrends(
            expenses);



        ApplyChartTheme();
    }



    // ==========================================
    // OVERVIEW
    // ==========================================

    private void LoadOverview(
        FinancialSnapshot snapshot)
    {
        // ======================================
        // CURRENT SPENDABLE CASH
        // ======================================

        OverviewSpendableCashLabel.Text =
            snapshot
                .CurrentSpendableCash
                .ToString("C");



        // ======================================
        // EXPECTED MONTHLY INCOME
        // ======================================

        OverviewIncomeLabel.Text =
            snapshot
                .ExpectedMonthlyIncome
                .ToString("C");



        // ======================================
        // SPENT THIS MONTH
        // ======================================

        OverviewSpentLabel.Text =
            snapshot
                .CurrentMonthSpent
                .ToString("C");



        // ======================================
        // UPCOMING BILLS
        // ======================================

        OverviewUpcomingBillsLabel.Text =
            snapshot
                .UpcomingBills
                .ToString("C");



        // ======================================
        // FINANCIAL HEALTH
        // ======================================

        UpdateFinancialHealth(
            snapshot);


        if (snapshot.DataConfidence.Equals(
                "Low",
                StringComparison.OrdinalIgnoreCase))
        {
            AnalyticsSafeToSpendTitleLabel.Text =
                "ESTIMATED SAFE TO SPEND";
        }
        else
        {
            AnalyticsSafeToSpendTitleLabel.Text =
                "SAFE TO SPEND";
        }
        // ======================================
        // SAFE TO SPEND
        // ======================================

        SafeToSpendLabel.Text =
            snapshot
                .SafeToSpendTotal
                .ToString("C");


        DailySafeLabel.Text =
            snapshot
                .SafeToSpendToday
                .ToString("C");


        WeeklySafeLabel.Text =
            snapshot
                .SafeToSpendThisWeek
                .ToString("C");


        UpdateWeeklySafeTitle();



        // ======================================
        // POCKETAI SUMMARY
        // ======================================

        UpdateOverviewInsight(
            snapshot);
    }



    // ==========================================
    // FINANCIAL HEALTH
    // ==========================================

    private void UpdateFinancialHealth(
        FinancialSnapshot snapshot)
    {
        if (!snapshot.HasEnoughDataForHealthScore ||
            !snapshot.FinancialHealthScore.HasValue)
        {
            HealthScoreLabel.Text =
                "Not enough data";


            HealthScoreLabel.FontSize =
                24;


            HealthStatusLabel.Text =
                $"{snapshot.DataConfidence} confidence";


            HealthReasonLabel.Text =
                snapshot.DataConfidenceReason;


            HealthProgressBar.Progress =
                0;


            HealthProgressBar.IsVisible =
                false;



            if (snapshot.DataConfidence.Equals(
                    "Low",
                    StringComparison.OrdinalIgnoreCase))
            {
                HealthStatusLabel
                    .SetDynamicResource(
                        Label.TextColorProperty,
                        "WarningColor");
            }
            else
            {
                HealthStatusLabel
                    .SetDynamicResource(
                        Label.TextColorProperty,
                        "ThemePrimary");
            }


            return;
        }



        int score =
            snapshot
                .FinancialHealthScore
                .Value;



        HealthScoreLabel.FontSize =
            32;


        HealthScoreLabel.Text =
            $"{score} / 100";


        HealthStatusLabel.Text =
            $"{GetFinancialHealthStatus(score)} • " +
            $"{snapshot.DataConfidence} confidence";


        HealthReasonLabel.Text =
            snapshot.DataConfidenceReason;


        HealthProgressBar.IsVisible =
            true;


        HealthProgressBar.Progress =
            score /
            100.0;



        if (score >= 70)
        {
            HealthStatusLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "SuccessColor");
        }
        else if (score >= 50)
        {
            HealthStatusLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "WarningColor");
        }
        else
        {
            HealthStatusLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "DangerColor");
        }
    }



    // ==========================================
    // HEALTH STATUS
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
    // WEEKLY SAFE TO SPEND TITLE
    // ==========================================

    private void UpdateWeeklySafeTitle()
    {
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



        if (daysLeft < 7)
        {
            DateTime endOfMonth =
                new DateTime(
                    today.Year,
                    today.Month,
                    daysInMonth);


            WeeklySafeTitleLabel.Text =
                $"THROUGH {endOfMonth:MMM d}"
                    .ToUpper();
        }
        else
        {
            WeeklySafeTitleLabel.Text =
                "THIS WEEK";
        }
    }



    // ==========================================
    // OVERVIEW INSIGHT
    // ==========================================

    private void UpdateOverviewInsight(
        FinancialSnapshot snapshot)
    {
        // ======================================
        // OBLIGATION SHORTFALL
        // ======================================

        if (snapshot.ObligationShortfall > 0)
        {
            OverviewInsightLabel.Text =
                $"Your current spendable cash is " +
                $"{snapshot.ObligationShortfall:C} short of your protected obligations. " +
                "Review upcoming bills, savings deadlines, or spending before making additional purchases.";


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
                OverviewInsightLabel.Text =
                    $"{snapshot.RequiredSavingsThisMonth:C} is already protected for required savings. " +
                    $"Based on the information entered so far, your estimated Safe to Spend is {snapshot.SafeToSpendTotal:C}.";
            }
            else
            {
                OverviewInsightLabel.Text =
                    $"Based on the information entered so far, your estimated Safe to Spend is {snapshot.SafeToSpendTotal:C}.";
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


            OverviewInsightLabel.Text =
                $"{snapshot.OverBudgetCount} budget {categoryText} currently over its limit. " +
                $"You still have {snapshot.SafeToSpendTotal:C} total Safe to Spend, " +
                "but reviewing those categories should come before extra discretionary spending.";


            return;
        }



        // ======================================
        // OPTIONAL EXTRA SAVINGS
        // ======================================

        if (snapshot.PocketAiRecommendedExtraSavings > 0)
        {
            OverviewInsightLabel.Text =
                $"You currently have {snapshot.SafeToSpendTotal:C} total Safe to Spend. " +
                $"Based on your current data, PocketAI could optionally recommend up to " +
                $"{snapshot.PocketAiRecommendedExtraSavings:C} extra toward savings. " +
                "That optional amount does not reduce Safe to Spend unless you choose to accept it.";


            return;
        }



        // ======================================
        // NORMAL STATE
        // ======================================

        if (snapshot.SafeToSpendTotal > 0)
        {
            OverviewInsightLabel.Text =
                $"You currently have {snapshot.SafeToSpendTotal:C} total Safe to Spend from " +
                $"{snapshot.CurrentSpendableCash:C} of current spendable cash.";


            return;
        }



        OverviewInsightLabel.Text =
            "Your current spendable cash is already committed to protected obligations and your safety buffer.";
    }



    // ==========================================
    // SPENDING TAB
    // ==========================================

    private void LoadSpending(
        List<Expense> expenses)
    {
        List<Expense> currentMonth =
            analyticsService
                .GetCurrentMonthExpense(
                    expenses);


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
                        string.IsNullOrWhiteSpace(
                            expense.Category)

                            ? "Uncategorized"

                            : expense.Category)
                .Select(
                    group =>
                        new CategorySpendingItem
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
                .ToList();



        double total =
            categories.Sum(
                item =>
                    item.Amount);



        CategoryDonutTotalLabel.Text =
            total.ToString("C");


        CategoryDonutCenter.IsVisible =
            total > 0;



        foreach (CategorySpendingItem item
                 in categories)
        {
            item.TotalSpending =
                total;
        }



        // ======================================
        // DONUT CHART
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


                    InnerRadius =
                        70,


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



        BindableLayout.SetItemsSource(
            CategorySpendingContainer,
            categories);


        CategoryEmptyLabel.IsVisible =
            categories.Count == 0;
    }



    // ==========================================
    // WEEK COMPARISON
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
                            Math.Max(
                                expense.Amount,
                                0));


            lastWeekValues[i] =
                lastWeek
                    .Where(
                        expense =>
                            expense.Date.DayOfWeek ==
                            day)
                    .Sum(
                        expense =>
                            Math.Max(
                                expense.Amount,
                                0));
        }



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



        UpdateSpendingComparison(
            WeeklyChangeLabel,
            thisWeekTotal,
            lastWeekTotal,
            "last week");



        WeekComparisonChart.Series =
            new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name =
                        "This Week",


                    Values =
                        thisWeekValues,


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


                    YToolTipLabelFormatter =
                        point =>
                            point.Model
                                .ToString("C2")
                }
            };



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
    // CASH FLOW / CURRENT CASH PROTECTION
    // ==========================================

    private void LoadCashFlow(
        FinancialSnapshot snapshot)
    {
        // ======================================
        // CURRENT MONEY PROTECTION CHART
        // ======================================
        //
        // This chart uses CURRENT money only.
        //
        // Expected income is NOT added here.
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
                            snapshot.CurrentSpendableCash,

                            -snapshot.UpcomingBills,

                            -snapshot.RequiredSavingsThisMonth,

                            -snapshot.AcceptedExtraSavings,

                            -snapshot.SafetyBuffer,

                            snapshot.SafeToSpendTotal
                        },


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
                            "Spendable",
                            "Bills",
                            "Required Savings",
                            "Accepted Extra",
                            "Buffer",
                            "Safe to Spend"
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
        // CURRENT CASH PROTECTION LABELS
        // ======================================

        CashFlowSpendableLabel.Text =
            snapshot
                .CurrentSpendableCash
                .ToString("C");


        CashFlowBillsLabel.Text =
            $"-{snapshot.UpcomingBills:C}";


        CashFlowRequiredSavingsLabel.Text =
            $"-{snapshot.RequiredSavingsThisMonth:C}";


        CashFlowAcceptedSavingsLabel.Text =
            $"-{snapshot.AcceptedExtraSavings:C}";


        CashFlowBufferLabel.Text =
            $"-{snapshot.SafetyBuffer:C}";


        CashFlowSafeLabel.Text =
            snapshot
                .SafeToSpendTotal
                .ToString("C");



        // ======================================
        // MONTHLY PLANNING
        // ======================================

        PlanIncomeLabel.Text =
            snapshot
                .ExpectedMonthlyIncome
                .ToString("C");


        PlanSpentLabel.Text =
            snapshot
                .CurrentMonthSpent
                .ToString("C");


        PlanRemainingLabel.Text =
            snapshot
                .MonthlyPlanRemaining
                .ToString("C");



        // ======================================
        // CURRENT-CASH MONTH-END PROJECTION
        // ======================================

        ProjectedMoneyLabel.Text =
            snapshot
                .ProjectedMonthEndSpendableCash
                .ToString("C");



        if (snapshot.DataConfidence.Equals(
                "Low",
                StringComparison.OrdinalIgnoreCase))
        {
            ProjectionMessageLabel.Text =
                $"This projection starts with your current spendable cash and known obligations. " +
                $"Expected monthly income is not counted until it is actually received. " +
                $"{snapshot.DataConfidenceReason}";


            return;
        }



        if (snapshot.ProjectedMonthEndSpendableCash < 0)
        {
            ProjectionMessageLabel.Text =
                $"Based on current spendable cash, known obligations, and your recorded spending pace, " +
                $"you may finish the month about " +
                $"{Math.Abs(snapshot.ProjectedMonthEndSpendableCash):C} short. " +
                "Expected future income is not included.";
        }
        else
        {
            ProjectionMessageLabel.Text =
                $"Based on current spendable cash, known obligations, and your recorded spending pace, " +
                $"you may finish the month with about " +
                $"{snapshot.ProjectedMonthEndSpendableCash:C} of spendable cash. " +
                "Expected future income is not included.";
        }
    }



    // ==========================================
    // SIX-MONTH TREND
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
                            Math.Max(
                                expense.Amount,
                                0));
        }



        SixMonthTrendChart.Series =
            new ISeries[]
            {
                new LineSeries<double>
                {
                    Name =
                        "Monthly Spending",


                    Values =
                        monthlyValues,


                    GeometrySize =
                        10,


                    LineSmoothness =
                        0.4,


                    Fill =
                        null,


                    YToolTipLabelFormatter =
                        point =>
                            point.Model
                                .ToString("C2")
                }
            };



        SixMonthTrendChart.XAxes =
            new Axis[]
            {
                new Axis
                {
                    Labels =
                        monthLabels
                }
            };



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



        double average =
            monthlyValues
                .Average();



        SixMonthAverageLabel.Text =
            average
                .ToString("C");



        double previousMonth =
            monthlyValues[4];


        double currentMonthAmount =
            monthlyValues[5];



        if (previousMonth <= 0 &&
            currentMonthAmount <= 0)
        {
            TrendDirectionLabel.Text =
                "Not enough data";


            TrendDirectionLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "TextSecondary");


            return;
        }



        if (currentMonthAmount >
            previousMonth)
        {
            double difference =
                currentMonthAmount -
                previousMonth;


            TrendDirectionLabel.Text =
                $"↑ {difference:C} more";


            TrendDirectionLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "DangerColor");


            return;
        }



        if (currentMonthAmount <
            previousMonth)
        {
            double difference =
                previousMonth -
                currentMonthAmount;


            TrendDirectionLabel.Text =
                $"↓ {difference:C} less";


            TrendDirectionLabel
                .SetDynamicResource(
                    Label.TextColorProperty,
                    "SuccessColor");


            return;
        }



        TrendDirectionLabel.Text =
            "→ Spending steady";


        TrendDirectionLabel
            .SetDynamicResource(
                Label.TextColorProperty,
                "TextSecondary");
    }



    // ==========================================
    // SPENDING COMPARISON
    // ==========================================

    private void UpdateSpendingComparison(
        Label label,
        double currentAmount,
        double previousAmount,
        string comparisonPeriod)
    {
        if (currentAmount <= 0 &&
            previousAmount <= 0)
        {
            label.Text =
                "No change";


            label.SetDynamicResource(
                Label.TextColorProperty,
                "TextSecondary");


            return;
        }



        if (previousAmount <= 0)
        {
            label.Text =
                $"New spending\nNo {comparisonPeriod} data";


            label.SetDynamicResource(
                Label.TextColorProperty,
                "TextSecondary");


            return;
        }



        double percentageChange =
            analyticsService
                .GetSpendingPercentageChange(
                    currentAmount,
                    previousAmount);



        double dollarDifference =
            currentAmount -
            previousAmount;



        if (percentageChange >= 200)
        {
            double multiplier =
                currentAmount /
                previousAmount;


            label.Text =
                $"Up {dollarDifference:C}\n" +
                $"{multiplier:0.0}× {comparisonPeriod}";


            label.SetDynamicResource(
                Label.TextColorProperty,
                "DangerColor");


            return;
        }



        if (percentageChange > 0)
        {
            label.Text =
                $"+{percentageChange:0.0}%\n" +
                $"vs {comparisonPeriod}";


            label.SetDynamicResource(
                Label.TextColorProperty,
                "DangerColor");


            return;
        }



        if (percentageChange < 0)
        {
            label.Text =
                $"{percentageChange:0.0}%\n" +
                $"vs {comparisonPeriod}";


            label.SetDynamicResource(
                Label.TextColorProperty,
                "SuccessColor");


            return;
        }



        label.Text =
            $"0%\nvs {comparisonPeriod}";


        label.SetDynamicResource(
            Label.TextColorProperty,
            "TextSecondary");
    }



    // ==========================================
    // TABS
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
    // SHOW TAB
    // ==========================================

    private void ShowTab(
        string tab)
    {
        currentTab =
            tab;


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
    // TAB STYLE
    // ==========================================

    private void SetTabButtonStyle(
        Button button,
        bool selected)
    {
        if (selected)
        {
            button.BackgroundColor =
                GetThemeColor(
                    "ThemePrimary",
                    "#7C3AED");


            button.TextColor =
                GetThemeColor(
                    "TextOnPrimary",
                    "#FFFFFF");


            return;
        }



        button.BackgroundColor =
            Colors.Transparent;


        button.TextColor =
            GetThemeColor(
                "TextSecondary",
                "#6B7280");
    }



    // ==========================================
    // THEME COLOR
    // ==========================================

    private static Color GetThemeColor(
        string resourceName,
        string fallbackColor)
    {
        if (Application.Current != null &&
            Application.Current.Resources[
                resourceName] is Color color)
        {
            return color;
        }


        return Color.FromArgb(
            fallbackColor);
    }



    // ==========================================
    // APPLY LIVECHARTS THEME
    // ==========================================

    private void ApplyChartTheme()
    {
        Color accentColor =
            GetThemeColor(
                "ThemePrimary",
                "#7C3AED");


        Color primaryTextColor =
            GetThemeColor(
                "TextPrimary",
                "#111827");


        Color secondaryTextColor =
            GetThemeColor(
                "TextSecondary",
                "#6B7280");


        Color mutedColor =
            GetThemeColor(
                "TextMuted",
                "#9CA3AF");


        Color borderColor =
            GetThemeColor(
                "BorderColor",
                "#E5E7EB");


        Color surfaceColor =
            GetThemeColor(
                "SurfaceBackground",
                "#F9FAFB");



        SKColor accent =
            ToSKColor(
                accentColor);


        SKColor primaryText =
            ToSKColor(
                primaryTextColor);


        SKColor secondaryText =
            ToSKColor(
                secondaryTextColor);


        SKColor muted =
            ToSKColor(
                mutedColor);


        SKColor border =
            ToSKColor(
                borderColor);


        SKColor surface =
            ToSKColor(
                surfaceColor);



        // ======================================
        // WEEK CHART
        // ======================================

        StyleCartesianChart(
            WeekComparisonChart,
            secondaryText,
            border,
            surface);



        List<ColumnSeries<double>>
            weekSeries =
                WeekComparisonChart
                    .Series
                    .OfType<
                        ColumnSeries<double>>()
                    .ToList();



        if (weekSeries.Count >= 1)
        {
            weekSeries[0].Fill =
                new SolidColorPaint(
                    accent);
        }


        if (weekSeries.Count >= 2)
        {
            weekSeries[1].Fill =
                new SolidColorPaint(
                    muted);
        }



        // ======================================
        // CASH FLOW CHART
        // ======================================

        StyleCartesianChart(
            CashFlowChart,
            secondaryText,
            border,
            surface);



        ColumnSeries<double>?
            cashFlowSeries =
                CashFlowChart
                    .Series
                    .OfType<
                        ColumnSeries<double>>()
                    .FirstOrDefault();



        if (cashFlowSeries != null)
        {
            cashFlowSeries.Fill =
                new SolidColorPaint(
                    accent);
        }



        // ======================================
        // TREND CHART
        // ======================================

        StyleCartesianChart(
            SixMonthTrendChart,
            secondaryText,
            border,
            surface);



        LineSeries<double>?
            trendSeries =
                SixMonthTrendChart
                    .Series
                    .OfType<
                        LineSeries<double>>()
                    .FirstOrDefault();



        if (trendSeries != null)
        {
            trendSeries.Stroke =
                new SolidColorPaint(
                    accent,
                    3);


            trendSeries.GeometryStroke =
                new SolidColorPaint(
                    accent,
                    2);


            trendSeries.GeometryFill =
                new SolidColorPaint(
                    accent);


            trendSeries.Fill =
                null;
        }



        // ======================================
        // DONUT
        // ======================================

        CategoryDonutChart.LegendTextPaint =
            new SolidColorPaint(
                secondaryText);


        CategoryDonutChart.TooltipTextPaint =
            new SolidColorPaint(
                primaryText);


        CategoryDonutChart.TooltipBackgroundPaint =
            new SolidColorPaint(
                surface);
    }



    // ==========================================
    // STYLE CARTESIAN CHART
    // ==========================================

    private static void StyleCartesianChart(
        LiveChartsCore.SkiaSharpView.Maui
            .CartesianChart chart,
        SKColor textColor,
        SKColor separatorColor,
        SKColor surfaceColor)
    {
        foreach (Axis axis
                 in chart.XAxes)
        {
            axis.LabelsPaint =
                new SolidColorPaint(
                    textColor);


            axis.SeparatorsPaint =
                new SolidColorPaint(
                    separatorColor)
                {
                    StrokeThickness =
                        1
                };
        }



        foreach (Axis axis
                 in chart.YAxes)
        {
            axis.LabelsPaint =
                new SolidColorPaint(
                    textColor);


            axis.SeparatorsPaint =
                new SolidColorPaint(
                    separatorColor)
                {
                    StrokeThickness =
                        1
                };
        }



        chart.LegendTextPaint =
            new SolidColorPaint(
                textColor);


        chart.TooltipTextPaint =
            new SolidColorPaint(
                textColor);


        chart.TooltipBackgroundPaint =
            new SolidColorPaint(
                surfaceColor);
    }



    // ==========================================
    // MAUI COLOR → SKIA COLOR
    // ==========================================

    private static SKColor ToSKColor(
        Color color)
    {
        byte red =
            (byte)(
                color.Red *
                255);


        byte green =
            (byte)(
                color.Green *
                255);


        byte blue =
            (byte)(
                color.Blue *
                255);


        byte alpha =
            (byte)(
                color.Alpha *
                255);



        return new SKColor(
            red,
            green,
            blue,
            alpha);
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



        public string AmountText =>
            Amount.ToString(
                "C");



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