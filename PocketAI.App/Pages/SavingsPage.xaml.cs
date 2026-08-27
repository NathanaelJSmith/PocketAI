using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PocketAI.App.Pages;

public partial class SavingsPage : ContentPage
{
    // ==========================================
    // SERVICES
    // ==========================================

    private readonly DataBaseManager dataBaseManager;

    private readonly AnalyticsService analyticsService;

    private readonly SavingsAllocationService
        savingsAllocationService;



    // ==========================================
    // SAVINGS GOALS
    // ==========================================

    private List<SavingsGoal> savingsGoals =
        new List<SavingsGoal>();


    private SavingsGoal? selectedGoal;



    // ==========================================
    // POCKETAI SAVINGS RECOMMENDATION
    // ==========================================

    // Amount currently being divided
    // between the user's savings goals.
    private double availableForSavings;


    // PocketAI's recommended amount to
    // keep available instead of allocating.
    private double recommendedSavingsBuffer;


    // PocketAI's projected amount remaining
    // at the end of the current month.
    private double projectedEndOfMonthMoney;


    // Decimal form.
    //
    // Example:
    // 0.30 = 30%
    private double
        recommendedSavingsBufferPercentage;


    // Explanation shown to the user.
    private string savingsBufferReason =
        "";


    // PocketAI's calculated savings amount
    // before a user manually adjusts it.
    private double pocketAiEstimatedSavings;


    // null:
    // use PocketAI's estimate.
    //
    // number:
    // use the amount manually selected
    // by the user.
    private double?
        userSavingsAmountOverride;


    // Current recommendation across all
    // active savings goals.
    private SavingsAllocationPlan?
        currentAllocationPlan;



    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public SavingsPage()
    {
        InitializeComponent();


        string databasePath =
            Path.Combine(
                FileSystem.AppDataDirectory,
                "pocketai.db");


        dataBaseManager =
            new DataBaseManager(
                databasePath);


        analyticsService =
            new AnalyticsService();


        savingsAllocationService =
            new SavingsAllocationService();


        dataBaseManager.CreateTables();
    }



    // ==========================================
    // PAGE APPEARS
    // ==========================================

    protected override void OnAppearing()
    {
        base.OnAppearing();


        LoadSavingsGoals();
    }



    // ==========================================
    // CALCULATE SAVINGS RECOMMENDATION
    // ==========================================

    private void CalculateSavingsRecommendation()
    {
        // ======================================
        // LOAD CURRENT FINANCIAL DATA
        // ======================================

        List<Expense> expenses =
            dataBaseManager
                .GetAllExpenses();


        Income? income =
            dataBaseManager
                .GetIncome();


        AccountBalance? accountBalance =
            dataBaseManager
                .GetAccountBalance();


        SavingsGoal? primarySavingsGoal =
            dataBaseManager
                .GetSavingsGoal();


        List<BudgetLimit> budgetLimits =
            dataBaseManager
                .GetBudgetLimits();


        List<RecurringExpenses> recurringExpenses =
            dataBaseManager
                .GetRecuringExpenses();



        // ======================================
        // BUILD FINANCIAL SUMMARY
        // ======================================

        FinancialSummary summary =
            analyticsService
                .BuildFinancialSummary(
                    expenses,
                    income,
                    accountBalance,
                    primarySavingsGoal,
                    budgetLimits,
                    recurringExpenses);



        // ======================================
        // DAYS LEFT IN CURRENT MONTH
        // ======================================

        DateTime today =
            DateTime.Today;


        int daysInMonth =
            DateTime.DaysInMonth(
                today.Year,
                today.Month);


        int daysLeftInMonth =
            Math.Max(
                daysInMonth -
                today.Day,
                0);



        // ======================================
        // CURRENT DAILY SPENDING RATE
        // ======================================

        double averageDailySpending =
            analyticsService
                .GetAverageDailySpending(
                    summary.CurrentMonthSpent,
                    today.Day);



        // ======================================
        // PROJECT REST-OF-MONTH SPENDING
        // ======================================

        double projectedAdditionalSpending =
            analyticsService
                .GetProjectedAdditionalSpending(
                    averageDailySpending,
                    daysLeftInMonth);



        // ======================================
        // PROJECT END-OF-MONTH MONEY
        // ======================================

        projectedEndOfMonthMoney =
            analyticsService
                .GetProjectedEndOfMonthMoney(
                    summary.MoneyLeft,
                    projectedAdditionalSpending);



        // ======================================
        // DYNAMIC BUFFER PERCENTAGE
        // ======================================

        recommendedSavingsBufferPercentage =
            savingsAllocationService
                .CalculateRecommendedBufferPercentage(
                    projectedEndOfMonthMoney,
                    summary.MonthlyIncome,
                    summary.MonthlyRecurringExpenses,
                    summary.OverBudgetCount);



        // ======================================
        // RECOMMENDED BUFFER
        // ======================================

        recommendedSavingsBuffer =
            savingsAllocationService
                .CalculateRecommendedBuffer(
                    projectedEndOfMonthMoney,
                    summary.MonthlyIncome,
                    summary.MonthlyRecurringExpenses,
                    summary.OverBudgetCount);



        // ======================================
        // POCKETAI'S AVAILABLE SAVINGS ESTIMATE
        // ======================================

        pocketAiEstimatedSavings =
            savingsAllocationService
                .CalculateAvailableForSavings(
                    projectedEndOfMonthMoney,
                    summary.MonthlyIncome,
                    summary.MonthlyRecurringExpenses,
                    summary.OverBudgetCount);



        // ======================================
        // AMOUNT ACTUALLY USED
        // ======================================
        //
        // PocketAI recommends an amount,
        // but the user remains in control.
        // ======================================

        availableForSavings =
            userSavingsAmountOverride
            ??
            pocketAiEstimatedSavings;



        // ======================================
        // BUFFER EXPLANATION
        // ======================================

        savingsBufferReason =
            BuildSavingsBufferReason(
                summary);



        // ======================================
        // DIVIDE AVAILABLE SAVINGS
        // ======================================

        currentAllocationPlan =
            savingsAllocationService
                .CalculateRecommendedAllocation(
                    savingsGoals,
                    availableForSavings);
    }



    // ==========================================
    // BUILD SAVINGS BUFFER EXPLANATION
    // ==========================================

    private string BuildSavingsBufferReason(
        FinancialSummary summary)
    {
        // ======================================
        // NO POSITIVE SURPLUS
        // ======================================

        if (projectedEndOfMonthMoney <= 0)
        {
            return
                "PocketAI is not recommending additional savings because your projected month-end money is too limited.";
        }



        int bufferPercent =
            (int)Math.Round(
                recommendedSavingsBufferPercentage
                *
                100);



        // ======================================
        // MONTHLY INCOME NOT SET
        // ======================================

        if (summary.MonthlyIncome <= 0)
        {
            return
                $"PocketAI is keeping " +
                $"{recommendedSavingsBuffer:C} " +
                $"({bufferPercent}%) as a buffer because reliable monthly income has not been set yet.";
        }



        // ======================================
        // FINANCIAL PRESSURE RATIOS
        // ======================================

        double recurringRatio =
            summary.MonthlyRecurringExpenses
            /
            summary.MonthlyIncome;


        double surplusRatio =
            projectedEndOfMonthMoney
            /
            summary.MonthlyIncome;



        // ======================================
        // HIGH FINANCIAL PRESSURE
        // ======================================

        if (summary.OverBudgetCount >= 2 &&
            recurringRatio >= 0.35)
        {
            return
                $"PocketAI is keeping " +
                $"{recommendedSavingsBuffer:C} " +
                $"({bufferPercent}%) available because several budgets are over their limits and recurring bills are using a significant part of your income.";
        }



        // ======================================
        // MULTIPLE BUDGETS OVER LIMIT
        // ======================================

        if (summary.OverBudgetCount >= 2)
        {
            return
                $"PocketAI is keeping " +
                $"{recommendedSavingsBuffer:C} " +
                $"({bufferPercent}%) as extra protection because multiple budget categories are currently over their limits.";
        }



        // ======================================
        // ONE BUDGET OVER LIMIT
        // ======================================

        if (summary.OverBudgetCount == 1)
        {
            return
                $"PocketAI is keeping " +
                $"{recommendedSavingsBuffer:C} " +
                $"({bufferPercent}%) available because one of your budget categories is currently over its limit.";
        }



        // ======================================
        // VERY HIGH RECURRING BILL PRESSURE
        // ======================================

        if (recurringRatio >= 0.50)
        {
            return
                $"PocketAI is keeping " +
                $"{recommendedSavingsBuffer:C} " +
                $"({bufferPercent}%) available because recurring bills are using a large portion of your monthly income.";
        }



        // ======================================
        // MODERATE RECURRING BILL PRESSURE
        // ======================================

        if (recurringRatio >= 0.35)
        {
            return
                $"PocketAI is keeping " +
                $"{recommendedSavingsBuffer:C} " +
                $"({bufferPercent}%) available because a significant portion of your income is already committed to recurring bills.";
        }



        // ======================================
        // VERY TIGHT MONTH
        // ======================================

        if (surplusRatio <= 0.10)
        {
            return
                $"PocketAI is keeping " +
                $"{recommendedSavingsBuffer:C} " +
                $"({bufferPercent}%) available because your projected month-end surplus is tight.";
        }



        // ======================================
        // TIGHT MONTH
        // ======================================

        if (surplusRatio <= 0.20)
        {
            return
                $"PocketAI is keeping " +
                $"{recommendedSavingsBuffer:C} " +
                $"({bufferPercent}%) as a larger safety cushion because this month has limited extra cash.";
        }



        // ======================================
        // BALANCED MONTH
        // ======================================

        if (surplusRatio <= 0.35)
        {
            return
                $"PocketAI is keeping " +
                $"{recommendedSavingsBuffer:C} " +
                $"({bufferPercent}%) available as a balanced financial buffer.";
        }



        // ======================================
        // STRONG MONTH
        // ======================================

        return
            $"PocketAI is keeping " +
            $"{recommendedSavingsBuffer:C} " +
            $"({bufferPercent}%) available as a safety buffer while putting more of your strong projected surplus toward savings.";
    }



    // ==========================================
    // LOAD SAVINGS GOALS
    // ==========================================

    private void LoadSavingsGoals()
    {
        // ======================================
        // LOAD GOALS FROM DATABASE
        // ======================================

        savingsGoals =
            dataBaseManager
                .GetSavingsGoals();



        // ======================================
        // SET UP PRIORITY DROPDOWN
        // ======================================

        SetupPriorityOptions();



        // ======================================
        // CALCULATE POCKETAI PLAN
        // ======================================

        CalculateSavingsRecommendation();



        // ======================================
        // AVAILABLE FOR SAVINGS DISPLAY
        // ======================================

        AvailableForSavingsLabel.Text =
            availableForSavings
                .ToString("C");



        // ======================================
        // BUFFER / CUSTOM AMOUNT EXPLANATION
        // ======================================

        if (userSavingsAmountOverride.HasValue)
        {
            if (availableForSavings >
                pocketAiEstimatedSavings)
            {
                SavingsBufferLabel.Text =
                    $"You chose {availableForSavings:C} for savings. " +
                    $"PocketAI's estimate is {pocketAiEstimatedSavings:C}, " +
                    $"so your custom amount uses more of the buffer PocketAI recommended keeping available.";
            }
            else if (availableForSavings <
                     pocketAiEstimatedSavings)
            {
                SavingsBufferLabel.Text =
                    $"You chose {availableForSavings:C} for savings. " +
                    $"PocketAI estimates you could save about {pocketAiEstimatedSavings:C}, " +
                    $"so your plan keeps additional money available.";
            }
            else
            {
                SavingsBufferLabel.Text =
                    $"Your custom amount matches PocketAI's current estimate of {pocketAiEstimatedSavings:C}.";
            }
        }
        else
        {
            SavingsBufferLabel.Text =
                savingsBufferReason;
        }



        // ======================================
        // BUILD GOAL DISPLAY ITEMS
        // ======================================

        List<SavingsGoalDisplayItem>
            displayItems =
                savingsGoals
                    .Select(
                        goal =>
                        {
                            SavingsAllocationItem?
                                allocation =
                                    currentAllocationPlan?
                                        .Allocations
                                        .FirstOrDefault(
                                            item =>
                                                item.GoalId
                                                ==
                                                goal.Id);


                            return
                                new SavingsGoalDisplayItem(
                                    goal,
                                    allocation,
                                    availableForSavings);
                        })
                    .ToList();



        BindableLayout.SetItemsSource(
            SavingsGoalsContainer,
            displayItems);



        // ======================================
        // EMPTY STATE
        // ======================================

        bool hasGoals =
            savingsGoals.Count > 0;


        SavingsEmptyState.IsVisible =
            !hasGoals;



        // ======================================
        // TOTAL SAVED
        // ======================================

        double totalSaved =
            savingsGoals.Sum(
                goal =>
                    goal.CurrentAmount);



        // ======================================
        // TOTAL TARGET
        // ======================================

        double totalTarget =
            savingsGoals.Sum(
                goal =>
                    goal.TargetAmount);



        // ======================================
        // TOTAL REMAINING
        // ======================================

        double totalRemaining =
            savingsGoals.Sum(
                goal =>
                    Math.Max(
                        goal.TargetAmount
                        -
                        goal.CurrentAmount,
                        0));



        TotalSavedLabel.Text =
            totalSaved.ToString("C");


        TotalTargetLabel.Text =
            totalTarget.ToString("C");


        TotalRemainingLabel.Text =
            totalRemaining.ToString("C");



        // ======================================
        // ACTIVE GOAL COUNT
        // ======================================

        int activeGoalCount =
            savingsGoals.Count(
                goal =>
                    goal.CurrentAmount
                    <
                    goal.TargetAmount);


        GoalCountLabel.Text =
            activeGoalCount
                .ToString();
    }



    // ==========================================
    // SET UP PRIORITY OPTIONS
    // ==========================================

    private void SetupPriorityOptions()
    {
        int highestPriority =
            savingsGoals
                .Where(
                    goal =>
                        goal.PriorityRank > 0)
                .Select(
                    goal =>
                        goal.PriorityRank)
                .DefaultIfEmpty(0)
                .Max();



        // Always provide at least five
        // priority levels.
        //
        // If the user has more goals or
        // already uses higher priorities,
        // automatically provide more.
        int numberOfPriorityOptions =
            Math.Max(
                5,
                Math.Max(
                    savingsGoals.Count + 1,
                    highestPriority + 1));



        List<string> priorityOptions =
            new List<string>();



        for (int priority = 1;
             priority <= numberOfPriorityOptions;
             priority++)
        {
            if (priority == 1)
            {
                priorityOptions.Add(
                    "Priority 1 — Highest");
            }
            else
            {
                priorityOptions.Add(
                    $"Priority {priority}");
            }
        }



        GoalPriorityPicker.ItemsSource =
            priorityOptions;
    }



    // ==========================================
    // SHOW ADD GOAL MODAL
    // ==========================================

    private void ShowAddGoalClicked(
        object? sender,
        EventArgs e)
    {
        selectedGoal =
            null;


        SetupPriorityOptions();



        GoalModalTitleLabel.Text =
            "ADD SAVINGS GOAL";


        SaveGoalButton.Text =
            "Add Goal";


        DeleteGoalButton.IsVisible =
            false;



        // ======================================
        // CLEAR FORM
        // ======================================

        GoalNameEntry.Text =
            "";


        GoalTargetEntry.Text =
            "";


        GoalCurrentEntry.Text =
            "0";


        GoalDeadlinePicker.Date =
            DateTime.Today
                .AddMonths(6);



        // ======================================
        // DEFAULT PRIORITY
        // ======================================
        //
        // A new goal begins after the
        // currently lowest-ranked tier.
        //
        // The user can freely choose another
        // Priority, including one already used
        // by another goal.
        // ======================================

        int nextPriority =
            savingsGoals
                .Where(
                    goal =>
                        goal.PriorityRank > 0)
                .Select(
                    goal =>
                        goal.PriorityRank)
                .DefaultIfEmpty(0)
                .Max()
            +
            1;



        GoalPriorityPicker.SelectedIndex =
            Math.Max(
                nextPriority - 1,
                0);



        // ======================================
        // DEFAULT ESSENTIAL STATUS
        // ======================================

        GoalEssentialSwitch.IsToggled =
            false;



        AddSavingsModal.IsVisible =
            false;


        AdjustSavingsModal.IsVisible =
            false;


        ModalBackground.IsVisible =
            true;


        GoalModal.IsVisible =
            true;
    }



    // ==========================================
    // SHOW EDIT GOAL MODAL
    // ==========================================

    private void ShowEditGoalClicked(
        object? sender,
        EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }


        if (button.BindingContext
            is not SavingsGoalDisplayItem item)
        {
            return;
        }



        selectedGoal =
            item.Goal;


        SetupPriorityOptions();



        GoalModalTitleLabel.Text =
            "EDIT SAVINGS GOAL";


        SaveGoalButton.Text =
            "Save Changes";


        DeleteGoalButton.IsVisible =
            true;



        // ======================================
        // CURRENT VALUES
        // ======================================

        GoalNameEntry.Text =
            selectedGoal.Name;


        GoalTargetEntry.Text =
            selectedGoal.TargetAmount
                .ToString("0.00");


        GoalCurrentEntry.Text =
            selectedGoal.CurrentAmount
                .ToString("0.00");


        GoalDeadlinePicker.Date =
            selectedGoal.DeadLine;



        // ======================================
        // CURRENT PRIORITY
        // ======================================

        int currentPriority =
            Math.Max(
                selectedGoal.PriorityRank,
                1);


        GoalPriorityPicker.SelectedIndex =
            currentPriority - 1;



        // ======================================
        // CURRENT ESSENTIAL STATUS
        // ======================================

        GoalEssentialSwitch.IsToggled =
            selectedGoal.IsEssential;



        AddSavingsModal.IsVisible =
            false;


        AdjustSavingsModal.IsVisible =
            false;


        ModalBackground.IsVisible =
            true;


        GoalModal.IsVisible =
            true;
    }



    // ==========================================
    // SAVE GOAL
    // ==========================================

    private async void SaveGoalClicked(
        object? sender,
        EventArgs e)
    {
        string name =
            GoalNameEntry.Text?
                .Trim()
            ??
            "";


        string targetText =
            GoalTargetEntry.Text?
                .Trim()
            ??
            "";


        string currentText =
            GoalCurrentEntry.Text?
                .Trim()
            ??
            "";



        // ======================================
        // NAME
        // ======================================

        if (string.IsNullOrWhiteSpace(
                name))
        {
            await DisplayAlertAsync(
                "Missing Name",
                "Enter a savings goal name.",
                "OK");


            return;
        }



        // ======================================
        // TARGET
        // ======================================

        if (!double.TryParse(
                targetText,
                out double targetAmount)
            ||
            targetAmount <= 0)
        {
            await DisplayAlertAsync(
                "Invalid Target",
                "Enter a valid target amount.",
                "OK");


            return;
        }



        // ======================================
        // CURRENT SAVINGS
        // ======================================

        if (!double.TryParse(
                currentText,
                out double currentAmount)
            ||
            currentAmount < 0)
        {
            await DisplayAlertAsync(
                "Invalid Savings",
                "Enter a valid amount already saved.",
                "OK");


            return;
        }



        // ======================================
        // DEADLINE
        // ======================================

        DateTime deadline =
            GoalDeadlinePicker.Date
            ??
            DateTime.Today
                .AddMonths(6);



        if (deadline.Date <
            DateTime.Today)
        {
            await DisplayAlertAsync(
                "Invalid Target Date",
                "Choose today or a future date.",
                "OK");


            return;
        }



        // ======================================
        // PRIORITY
        // ======================================

        if (GoalPriorityPicker.SelectedIndex < 0)
        {
            await DisplayAlertAsync(
                "Missing Priority",
                "Choose a priority level for this savings goal.",
                "OK");


            return;
        }



        int priorityRank =
            GoalPriorityPicker.SelectedIndex
            +
            1;



        // ======================================
        // ESSENTIAL
        // ======================================

        bool isEssential =
            GoalEssentialSwitch.IsToggled;



        // ======================================
        // ADD NEW GOAL
        // ======================================

        if (selectedGoal == null)
        {
            SavingsGoal newGoal =
                new SavingsGoal(
                    name,
                    targetAmount,
                    currentAmount,
                    deadline);



            newGoal.PriorityRank =
                priorityRank;


            newGoal.IsEssential =
                isEssential;


            // null means PocketAI currently
            // controls the recommended split.
            newGoal.CustomAllocationPercentage =
                null;



            dataBaseManager.AddSavingsGoal(
                newGoal);
        }



        // ======================================
        // UPDATE EXISTING GOAL
        // ======================================

        else
        {
            SavingsGoal updatedGoal =
                new SavingsGoal(
                    selectedGoal.Id,
                    name,
                    targetAmount,
                    currentAmount,
                    deadline,

                    // Controls which goal
                    // appears on Home.
                    selectedGoal.IsPrimary,

                    // Financial importance.
                    priorityRank,

                    // Essential protection.
                    isEssential,

                    // Preserve a future custom
                    // allocation percentage.
                    selectedGoal
                        .CustomAllocationPercentage);



            dataBaseManager.UpdateSavingsGoal(
                updatedGoal);
        }



        CloseSavingsModals();


        LoadSavingsGoals();
    }



    // ==========================================
    // SHOW ADD SAVINGS MODAL
    // ==========================================

    private void ShowAddSavingsClicked(
        object? sender,
        EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }


        if (button.BindingContext
            is not SavingsGoalDisplayItem item)
        {
            return;
        }



        selectedGoal =
            item.Goal;


        AddSavingsGoalNameLabel.Text =
            selectedGoal.Name;


        AddSavingsAmountEntry.Text =
            "";



        GoalModal.IsVisible =
            false;


        AdjustSavingsModal.IsVisible =
            false;


        ModalBackground.IsVisible =
            true;


        AddSavingsModal.IsVisible =
            true;
    }



    // ==========================================
    // ADD SAVINGS TO GOAL
    // ==========================================

    private async void AddSavingsClicked(
        object? sender,
        EventArgs e)
    {
        if (selectedGoal == null)
        {
            return;
        }



        string amountText =
            AddSavingsAmountEntry.Text?
                .Trim()
            ??
            "";



        if (!double.TryParse(
                amountText,
                out double amount)
            ||
            amount <= 0)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Enter a valid amount to add.",
                "OK");


            return;
        }



        double newCurrentAmount =
            selectedGoal.CurrentAmount
            +
            amount;



        // Preserve all Priority Savings
        // information while updating progress.
        SavingsGoal updatedGoal =
            new SavingsGoal(
                selectedGoal.Id,
                selectedGoal.Name,
                selectedGoal.TargetAmount,
                newCurrentAmount,
                selectedGoal.DeadLine,
                selectedGoal.IsPrimary,
                selectedGoal.PriorityRank,
                selectedGoal.IsEssential,
                selectedGoal
                    .CustomAllocationPercentage);



        dataBaseManager.UpdateSavingsGoal(
            updatedGoal);



        CloseSavingsModals();


        LoadSavingsGoals();
    }



    // ==========================================
    // SHOW GOAL ON HOME
    // ==========================================

    private void MakePrimaryClicked(
        object? sender,
        EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }


        if (button.BindingContext
            is not SavingsGoalDisplayItem item)
        {
            return;
        }



        // Internal property remains IsPrimary
        // for backwards compatibility.
        //
        // In the UI this is called
        // "Shown on Home".
        dataBaseManager
            .SetPrimarySavingsGoal(
                item.Goal.Id);



        LoadSavingsGoals();
    }



    // ==========================================
    // DELETE GOAL
    // ==========================================

    private async void DeleteGoalClicked(
        object? sender,
        EventArgs e)
    {
        if (selectedGoal == null)
        {
            return;
        }



        bool confirmed =
            await DisplayAlertAsync(
                "Delete Savings Goal",
                $"Delete {selectedGoal.Name}?",
                "Delete",
                "Cancel");



        if (!confirmed)
        {
            return;
        }



        dataBaseManager
            .DeleteSavingsGoalById(
                selectedGoal.Id);



        CloseSavingsModals();


        LoadSavingsGoals();
    }



    // ==========================================
    // ADJUST AVAILABLE SAVINGS
    // ==========================================

    private void AdjustSavingsAmountClicked(
        object? sender,
        EventArgs e)
    {
        // Start with the amount currently
        // being used by the recommendation.
        AdjustSavingsAmountEntry.Text =
            availableForSavings
                .ToString("0.00");



        GoalModal.IsVisible =
            false;


        AddSavingsModal.IsVisible =
            false;


        ModalBackground.IsVisible =
            true;


        AdjustSavingsModal.IsVisible =
            true;
    }



    // ==========================================
    // SAVE ADJUSTED SAVINGS AMOUNT
    // ==========================================

    private async void SaveAdjustedSavingsAmountClicked(
        object? sender,
        EventArgs e)
    {
        string amountText =
            AdjustSavingsAmountEntry.Text?
                .Trim()
            ??
            "";



        if (!double.TryParse(
                amountText,
                out double amount)
            ||
            amount < 0)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Enter a valid savings amount.",
                "OK");


            return;
        }



        // Respect the user's choice.
        //
        // This affects the recommendation only.
        // No money is transferred.
        userSavingsAmountOverride =
            amount;



        AdjustSavingsModal.IsVisible =
            false;


        ModalBackground.IsVisible =
            false;



        LoadSavingsGoals();
    }



    // ==========================================
    // USE POCKETAI ESTIMATE
    // ==========================================

    private void UsePocketAiSavingsEstimateClicked(
        object? sender,
        EventArgs e)
    {
        // null means return to PocketAI's
        // calculated amount.
        userSavingsAmountOverride =
            null;



        AdjustSavingsModal.IsVisible =
            false;


        ModalBackground.IsVisible =
            false;



        LoadSavingsGoals();
    }



    // ==========================================
    // CANCEL / CLOSE MODALS
    // ==========================================

    private void CancelSavingsModalClicked(
        object? sender,
        EventArgs e)
    {
        CloseSavingsModals();
    }



    // ==========================================
    // CLICK OUTSIDE MODAL
    // ==========================================

    private void CloseSavingsModalsClicked(
        object? sender,
        TappedEventArgs e)
    {
        CloseSavingsModals();
    }



    // ==========================================
    // CLOSE ALL SAVINGS MODALS
    // ==========================================

    private void CloseSavingsModals()
    {
        GoalModal.IsVisible =
            false;


        AddSavingsModal.IsVisible =
            false;


        AdjustSavingsModal.IsVisible =
            false;


        ModalBackground.IsVisible =
            false;


        selectedGoal =
            null;
    }



    // ==========================================
    // GET THEME COLOR
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
    // SAVINGS GOAL DISPLAY ITEM
    // ==========================================

    public class SavingsGoalDisplayItem
    {
        // ======================================
        // REAL SAVINGS GOAL
        // ======================================

        public SavingsGoal Goal
        {
            get;
        }



        // Recommendation for this specific goal.
        private readonly SavingsAllocationItem?
            allocation;



        // Total amount currently available
        // to divide between goals.
        private readonly double
            availableForSavings;



        // ======================================
        // BASIC INFORMATION
        // ======================================

        public string Name =>
            Goal.Name;



        // ======================================
        // SHOWN ON HOME
        // ======================================

        public bool IsPrimary =>
            Goal.IsPrimary;


        public bool CanMakePrimary =>
            !Goal.IsPrimary;



        // ======================================
        // PRIORITY
        // ======================================

        public int PriorityRank =>
            Goal.PriorityRank;


        public string PriorityText
        {
            get
            {
                if (Goal.PriorityRank <= 0)
                {
                    return
                        "UNRANKED";
                }


                return
                    $"PRIORITY {Goal.PriorityRank}";
            }
        }



        public Color PriorityColor =>
            GetThemeColor(
                "ThemePrimary",
                "#7C3AED");



        // ======================================
        // ESSENTIAL / OPTIONAL
        // ======================================

        public bool IsEssential =>
            Goal.IsEssential;


        public string EssentialText =>
            Goal.IsEssential
                ? "ESSENTIAL"
                : "OPTIONAL";


        public Color EssentialColor
        {
            get
            {
                if (Goal.IsEssential)
                {
                    return GetThemeColor(
                        "WarningColor",
                        "#B45309");
                }


                return GetThemeColor(
                    "TextSecondary",
                    "#6B7280");
            }
        }



        // ======================================
        // REMAINING
        // ======================================

        public double Remaining =>
            Math.Max(
                Goal.TargetAmount
                -
                Goal.CurrentAmount,
                0);



        // ======================================
        // COMPLETED
        // ======================================

        public bool IsCompleted =>
            Remaining <= 0;



        // ======================================
        // PERCENT COMPLETE
        // ======================================

        public double Percent
        {
            get
            {
                if (Goal.TargetAmount <= 0)
                {
                    return 0;
                }



                return Math.Clamp(
                    (
                        Goal.CurrentAmount
                        /
                        Goal.TargetAmount
                    )
                    *
                    100,
                    0,
                    100);
            }
        }



        public double Progress =>
            Percent
            /
            100.0;



        // ======================================
        // DAYS LEFT
        // ======================================

        public double DaysLeft =>
            (
                Goal.DeadLine.Date
                -
                DateTime.Today
            )
            .TotalDays;



        // ======================================
        // WEEKLY AMOUNT NEEDED
        // ======================================

        public double WeeklyNeeded
        {
            get
            {
                if (Remaining <= 0 ||
                    DaysLeft <= 0)
                {
                    return 0;
                }



                double weeksLeft =
                    DaysLeft
                    /
                    7.0;



                if (weeksLeft <= 0)
                {
                    return 0;
                }



                return
                    Remaining
                    /
                    weeksLeft;
            }
        }



        // ======================================
        // DISPLAY TEXT
        // ======================================

        public string DeadlineText =>
            $"Target: {Goal.DeadLine:MMM d, yyyy}";


        public string AmountText =>
            $"{Goal.CurrentAmount:C} / {Goal.TargetAmount:C}";


        public string PercentText =>
            $"{Percent:F0}%";


        public string RemainingText =>
            Remaining.ToString("C");



        public string DaysLeftText
        {
            get
            {
                if (IsCompleted)
                {
                    return
                        "Complete";
                }



                if (DaysLeft <= 0)
                {
                    return
                        "Date reached";
                }



                int days =
                    Math.Max(
                        (int)Math.Ceiling(
                            DaysLeft),
                        0);



                return
                    $"{days} days";
            }
        }



        public string WeeklyNeededText
        {
            get
            {
                if (IsCompleted)
                {
                    return
                        "Complete";
                }


                if (DaysLeft <= 0)
                {
                    return
                        "—";
                }


                return
                    WeeklyNeeded
                        .ToString("C");
            }
        }



        // ======================================
        // POCKETAI RECOMMENDED AMOUNT
        // ======================================

        public double RecommendedAmount =>
            allocation?
                .RecommendedAmount
            ??
            0;



        // ======================================
        // POCKETAI RECOMMENDED PERCENTAGE
        // ======================================

        public double RecommendedPercentage =>
            allocation?
                .RecommendedPercentage
            ??
            0;



        // ======================================
        // RECOMMENDED AMOUNT TEXT
        // ======================================

        public string RecommendedAmountText
        {
            get
            {
                if (IsCompleted)
                {
                    return
                        "Goal complete";
                }



                if (availableForSavings <= 0)
                {
                    return
                        "$0.00 this month";
                }



                return
                    $"{RecommendedAmount:C} this month";
            }
        }



        // ======================================
        // RECOMMENDED PERCENTAGE TEXT
        // ======================================

        public string RecommendedPercentageText
        {
            get
            {
                if (IsCompleted)
                {
                    return
                        "";
                }



                if (availableForSavings <= 0)
                {
                    return
                        "0% of available savings";
                }



                return
                    $"{RecommendedPercentage:F0}% of available savings";
            }
        }



        // ======================================
        // WHY POCKETAI RECOMMENDED IT
        // ======================================

        public string RecommendationReason
        {
            get
            {
                if (IsCompleted)
                {
                    return
                        "This goal is already fully funded, so PocketAI does not allocate additional savings to it.";
                }



                if (availableForSavings <= 0)
                {
                    return
                        "PocketAI is not recommending a contribution right now because your projected month-end finances are too limited.";
                }



                if (Goal.PriorityRank == 1 &&
                    Goal.IsEssential)
                {
                    return
                        "This goal receives extra weight because it is Priority 1 and marked Essential.";
                }



                if (Goal.IsEssential &&
                    Goal.PriorityRank > 0)
                {
                    return
                        $"This goal is Priority {Goal.PriorityRank} and Essential, so PocketAI gives it extra protection.";
                }



                if (Goal.PriorityRank == 1)
                {
                    return
                        "This goal receives a larger share because it is Priority 1.";
                }



                if (Goal.PriorityRank > 0)
                {
                    return
                        $"This recommendation reflects its Priority {Goal.PriorityRank} level.";
                }



                return
                    "This goal is currently unranked, so PocketAI treats it as a lower-priority goal.";
            }
        }



        // ======================================
        // STATUS
        // ======================================

        public string StatusText
        {
            get
            {
                if (IsCompleted)
                {
                    return
                        "✓ Goal completed";
                }



                if (DaysLeft <= 0)
                {
                    return
                        $"⚠ Target date passed • {Remaining:C} remaining";
                }



                // Keep this short.
                //
                // Needed / Week already shows
                // the deadline requirement.
                //
                // PocketAI Recommendation shows
                // what finances support.
                return
                    "Goal in progress";
            }
        }



        // ======================================
        // STATUS COLOR
        // ======================================

        public Color StatusColor
        {
            get
            {
                if (IsCompleted)
                {
                    return GetThemeColor(
                        "SuccessColor",
                        "#15803D");
                }



                if (DaysLeft <= 0)
                {
                    return GetThemeColor(
                        "DangerColor",
                        "#B91C1C");
                }



                return GetThemeColor(
                    "SuccessColor",
                    "#15803D");
            }
        }



        // ======================================
        // CONSTRUCTOR
        // ======================================

        public SavingsGoalDisplayItem(
            SavingsGoal goal,
            SavingsAllocationItem? allocation,
            double availableForSavings)
        {
            Goal =
                goal;


            this.allocation =
                allocation;


            this.availableForSavings =
                availableForSavings;
        }
    }
}