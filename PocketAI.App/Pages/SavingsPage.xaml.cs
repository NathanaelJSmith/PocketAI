using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PocketAI.App.Pages;

public partial class SavingsPage : ContentPage
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
    // SAVINGS ALLOCATION ENGINE
    // ==========================================
    //
    // This service does NOT decide how much
    // money the user can afford to save.
    //
    // Its job is only:
    //
    // "Given $X of OPTIONAL extra savings,
    // how should it be divided among goals?"
    // ==========================================

    private readonly SavingsAllocationService
        savingsAllocationService;



    // ==========================================
    // SAVINGS GOALS
    // ==========================================

    private List<SavingsGoal> savingsGoals =
        new List<SavingsGoal>();


    private SavingsGoal? selectedGoal;



    // ==========================================
    // CURRENT FINANCIAL SNAPSHOT
    // ==========================================

    private FinancialSnapshot?
        currentSnapshot;



    // ==========================================
    // OPTIONAL EXTRA SAVINGS
    // ==========================================
    //
    // Required savings are calculated by the
    // central FinancialCalculationService.
    //
    // This value is ONLY optional extra savings
    // beyond that required amount.
    // ==========================================

    private double pocketAiEstimatedExtraSavings;


    // Amount currently being PREVIEWED for
    // optional extra savings allocation.
    private double extraSavingsForAllocation;


    // null:
    // use PocketAI's recommendation.
    //
    // number:
    // preview a user-selected extra amount.
    //
    // IMPORTANT:
    // This is NOT accepted yet and therefore
    // does NOT reduce Safe to Spend.
    private double?
        userExtraSavingsPreviewOverride;



    // ==========================================
    // CURRENT ALLOCATION PLAN
    // ==========================================

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


        dataBaseManager.CreateTables();


        financialSnapshotProvider =
            new FinancialSnapshotProvider(
                dataBaseManager);


        savingsAllocationService =
            new SavingsAllocationService();
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
    // LOAD SAVINGS PAGE
    // ==========================================

    private void LoadSavingsGoals()
    {
        // ======================================
        // LOAD GOALS
        // ======================================

        savingsGoals =
            dataBaseManager
                .GetSavingsGoals();



        SetupPriorityOptions();



        // ======================================
        // GET CENTRAL FINANCIAL SNAPSHOT
        // ======================================
        //
        // Savings no longer calculates its own:
        //
        // - Money Left
        // - month-end surplus
        // - savings buffer
        // - available money
        //
        // Home, Analytics, and Savings now use
        // the same financial definitions.
        // ======================================

        currentSnapshot =
            financialSnapshotProvider
                .GetSnapshot();



        // ======================================
        // OPTIONAL EXTRA SAVINGS
        // ======================================

        pocketAiEstimatedExtraSavings =
            currentSnapshot
                .PocketAiRecommendedExtraSavings;



        extraSavingsForAllocation =
            userExtraSavingsPreviewOverride
            ??
            pocketAiEstimatedExtraSavings;



        // ======================================
        // DIVIDE OPTIONAL EXTRA AMONG GOALS
        // ======================================

        currentAllocationPlan =
            savingsAllocationService
                .CalculateRecommendedAllocation(
                    savingsGoals,
                    extraSavingsForAllocation);



        // ======================================
        // TOP SAVINGS PLAN
        // ======================================

        UpdateSavingsPlanSummary(
            currentSnapshot);



        // ======================================
        // GOAL DISPLAY ITEMS
        // ======================================

        string recommendationUnavailableReason =
            BuildRecommendationUnavailableReason(
                currentSnapshot);



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
                                                item.GoalId ==
                                                goal.Id);


                            return
                                new SavingsGoalDisplayItem(
                                    goal,
                                    allocation,
                                    extraSavingsForAllocation,
                                    userExtraSavingsPreviewOverride
                                        .HasValue,
                                    recommendationUnavailableReason);
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
        // SUMMARY CARDS
        // ======================================

        double totalSaved =
            savingsGoals.Sum(
                goal =>
                    goal.CurrentAmount);


        double totalTarget =
            savingsGoals.Sum(
                goal =>
                    goal.TargetAmount);


        double totalRemaining =
            savingsGoals.Sum(
                goal =>
                    Math.Max(
                        goal.TargetAmount
                        -
                        goal.CurrentAmount,
                        0));


        int activeGoalCount =
            savingsGoals.Count(
                goal =>
                    goal.CurrentAmount
                    <
                    goal.TargetAmount);



        TotalSavedLabel.Text =
            totalSaved
                .ToString("C");


        TotalTargetLabel.Text =
            totalTarget
                .ToString("C");


        TotalRemainingLabel.Text =
            totalRemaining
                .ToString("C");


        GoalCountLabel.Text =
            activeGoalCount
                .ToString();
    }



    // ==========================================
    // UPDATE SAVINGS PLAN SUMMARY
    // ==========================================

    private void UpdateSavingsPlanSummary(
        FinancialSnapshot snapshot)
    {
        DateTime today =
            DateTime.Today;


        int daysInMonth =
            DateTime.DaysInMonth(
                today.Year,
                today.Month);


        DateTime endOfMonth =
            new DateTime(
                today.Year,
                today.Month,
                daysInMonth);



        RequiredSavingsPeriodLabel.Text =
            $"REQUIRED THROUGH {endOfMonth:MMM d}"
                .ToUpper();



        RequiredSavingsThisMonthLabel.Text =
            snapshot
                .RequiredSavingsThisMonth
                .ToString("C");



        OptionalExtraSavingsLabel.Text =
            extraSavingsForAllocation
                .ToString("C");



        SavingsSafeToSpendLabel.Text =
            snapshot
                .SafeToSpendTotal
                .ToString("C");



        // ======================================
        // CUSTOM PREVIEW
        // ======================================

        if (userExtraSavingsPreviewOverride
            .HasValue)
        {
            OptionalExtraModeLabel.Text =
                "Custom preview";


            if (extraSavingsForAllocation >
                pocketAiEstimatedExtraSavings)
            {
                SavingsPlanExplanationLabel.Text =
                    $"You're previewing {extraSavingsForAllocation:C} of optional extra savings. " +
                    $"PocketAI currently recommends {pocketAiEstimatedExtraSavings:C}. " +
                    "This is only a preview and does not reduce Safe to Spend.";
            }
            else if (extraSavingsForAllocation <
                     pocketAiEstimatedExtraSavings)
            {
                SavingsPlanExplanationLabel.Text =
                    $"You're previewing {extraSavingsForAllocation:C} of optional extra savings. " +
                    $"PocketAI currently recommends {pocketAiEstimatedExtraSavings:C}. " +
                    "This preview keeps more discretionary money available.";
            }
            else
            {
                SavingsPlanExplanationLabel.Text =
                    $"Your preview matches PocketAI's optional recommendation of " +
                    $"{pocketAiEstimatedExtraSavings:C}. " +
                    "It has not been accepted or removed from Safe to Spend.";
            }


            return;
        }



        // ======================================
        // POCKETAI MODE
        // ======================================

        OptionalExtraModeLabel.Text =
            "PocketAI estimate";



        if (snapshot.ObligationShortfall > 0)
        {
            SavingsPlanExplanationLabel.Text =
                $"PocketAI is already protecting {snapshot.RequiredSavingsThisMonth:C} " +
                $"of required savings, but your current spendable cash is " +
                $"{snapshot.ObligationShortfall:C} short of protected obligations. " +
                "No optional extra savings should be added right now.";


            return;
        }



        if (pocketAiEstimatedExtraSavings <= 0)
        {
            if (snapshot.DataConfidence.Equals(
                    "Low",
                    StringComparison.OrdinalIgnoreCase))
            {
                SavingsPlanExplanationLabel.Text =
                    $"{snapshot.RequiredSavingsThisMonth:C} is already protected to keep your goals on schedule. " +
                    "PocketAI is not recommending extra savings yet because it needs more financial history. " +
                    snapshot.DataConfidenceReason;


                return;
            }



            SavingsPlanExplanationLabel.Text =
                $"{snapshot.RequiredSavingsThisMonth:C} is already protected for required savings. " +
                "PocketAI is not recommending additional optional savings right now.";


            return;
        }



        SavingsPlanExplanationLabel.Text =
            $"{snapshot.RequiredSavingsThisMonth:C} is already protected as required savings. " +
            $"PocketAI optionally recommends another {pocketAiEstimatedExtraSavings:C}. " +
            "Optional extra savings do not reduce Safe to Spend until you choose to accept them.";
    }



    // ==========================================
    // RECOMMENDATION UNAVAILABLE REASON
    // ==========================================

    private string BuildRecommendationUnavailableReason(
        FinancialSnapshot snapshot)
    {
        if (snapshot.ObligationShortfall > 0)
        {
            return
                "PocketAI is not recommending optional extra savings because current protected obligations exceed spendable cash.";
        }



        if (snapshot.DataConfidence.Equals(
                "Low",
                StringComparison.OrdinalIgnoreCase))
        {
            return
                "PocketAI needs more financial history before recommending optional extra savings. Required savings are already protected separately.";
        }



        if (snapshot.SafeToSpendTotal <= 0)
        {
            return
                "There is currently no Safe to Spend available for optional extra savings.";
        }



        return
            "PocketAI is not recommending additional optional savings right now.";
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
    // SHOW ADD GOAL
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



        GoalNameEntry.Text =
            "";


        GoalTargetEntry.Text =
            "";


        GoalCurrentEntry.Text =
            "0";


        GoalDeadlinePicker.Date =
            DateTime.Today
                .AddMonths(6);



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
    // SHOW EDIT GOAL
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



        int currentPriority =
            Math.Max(
                selectedGoal.PriorityRank,
                1);


        GoalPriorityPicker.SelectedIndex =
            currentPriority - 1;



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



        if (string.IsNullOrWhiteSpace(
                name))
        {
            await DisplayAlertAsync(
                "Missing Name",
                "Enter a savings goal name.",
                "OK");


            return;
        }



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



        bool isEssential =
            GoalEssentialSwitch.IsToggled;



        // ======================================
        // NEW GOAL
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


            newGoal.CustomAllocationPercentage =
                null;



            dataBaseManager
                .AddSavingsGoal(
                    newGoal);
        }



        // ======================================
        // EDIT GOAL
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
                    selectedGoal.IsPrimary,
                    priorityRank,
                    isEssential,
                    selectedGoal
                        .CustomAllocationPercentage);



            dataBaseManager
                .UpdateSavingsGoal(
                    updatedGoal);
        }



        CloseSavingsModals();


        LoadSavingsGoals();
    }



    // ==========================================
    // SHOW ADD SAVINGS
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
    // ADD SAVINGS
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



        double remaining =
            Math.Max(
                selectedGoal.TargetAmount
                -
                selectedGoal.CurrentAmount,
                0);



        if (remaining <= 0)
        {
            await DisplayAlertAsync(
                "Goal Complete",
                "This savings goal is already fully funded.",
                "OK");


            return;
        }



        if (amount > remaining)
        {
            await DisplayAlertAsync(
                "Amount Too Large",
                $"This goal only needs {remaining:C} to reach its target.",
                "OK");


            return;
        }



        double newCurrentAmount =
            selectedGoal.CurrentAmount
            +
            amount;



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



        dataBaseManager
            .UpdateSavingsGoal(
                updatedGoal);



        CloseSavingsModals();


        LoadSavingsGoals();
    }



    // ==========================================
    // SHOW ON HOME
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
    // ADJUST OPTIONAL EXTRA PREVIEW
    // ==========================================

    private void AdjustSavingsAmountClicked(
        object? sender,
        EventArgs e)
    {
        AdjustSavingsAmountEntry.Text =
            extraSavingsForAllocation
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
    // SAVE OPTIONAL EXTRA PREVIEW
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
                "Enter a valid optional extra savings amount.",
                "OK");


            return;
        }



        FinancialSnapshot snapshot =
            currentSnapshot
            ??
            financialSnapshotProvider
                .GetSnapshot();



        // ======================================
        // DO NOT PREVIEW MORE THAN SAFE TO SPEND
        // ======================================

        if (amount >
            snapshot.SafeToSpendTotal)
        {
            await DisplayAlertAsync(
                "Amount Exceeds Safe to Spend",
                $"Your current total Safe to Spend is {snapshot.SafeToSpendTotal:C}. " +
                "Choose an optional extra amount at or below that value.",
                "OK");


            return;
        }



        userExtraSavingsPreviewOverride =
            amount;



        AdjustSavingsModal.IsVisible =
            false;


        ModalBackground.IsVisible =
            false;



        LoadSavingsGoals();
    }



    // ==========================================
    // RESET TO POCKETAI EXTRA ESTIMATE
    // ==========================================

    private void UsePocketAiSavingsEstimateClicked(
        object? sender,
        EventArgs e)
    {
        userExtraSavingsPreviewOverride =
            null;



        AdjustSavingsModal.IsVisible =
            false;


        ModalBackground.IsVisible =
            false;



        LoadSavingsGoals();
    }



    // ==========================================
    // CANCEL MODAL
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
    // CLOSE ALL MODALS
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
    // SAVINGS GOAL DISPLAY ITEM
    // ==========================================

    public class SavingsGoalDisplayItem
    {
        public SavingsGoal Goal
        {
            get;
        }



        private readonly SavingsAllocationItem?
            allocation;


        private readonly double
            optionalExtraSavings;


        private readonly bool
            isCustomPreview;


        private readonly string
            recommendationUnavailableReason;



        // ======================================
        // BASIC INFO
        // ======================================

        public string Name =>
            Goal.Name;


        public bool IsPrimary =>
            Goal.IsPrimary;


        public bool CanMakePrimary =>
            !Goal.IsPrimary;



        // ======================================
        // PRIORITY
        // ======================================

        public int PriorityRank =>
            Goal.PriorityRank;


        public string PriorityText =>
            Goal.PriorityRank > 0
                ? $"PRIORITY {Goal.PriorityRank}"
                : "UNRANKED";


        public Color PriorityColor =>
            GetThemeColor(
                "ThemePrimary",
                "#7C3AED");



        // ======================================
        // ESSENTIAL
        // ======================================

        public bool IsEssential =>
            Goal.IsEssential;


        public string EssentialText =>
            Goal.IsEssential
                ? "ESSENTIAL"
                : "OPTIONAL";


        public Color EssentialColor =>
            Goal.IsEssential

                ? GetThemeColor(
                    "WarningColor",
                    "#B45309")

                : GetThemeColor(
                    "TextSecondary",
                    "#6B7280");



        // ======================================
        // REMAINING / COMPLETE
        // ======================================

        public double Remaining =>
            Math.Max(
                Goal.TargetAmount
                -
                Goal.CurrentAmount,
                0);


        public bool IsCompleted =>
            Remaining <= 0;



        // ======================================
        // PROGRESS
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
                    Goal.CurrentAmount
                    /
                    Goal.TargetAmount
                    *
                    100,
                    0,
                    100);
            }
        }


        public double Progress =>
            Percent /
            100.0;



        // ======================================
        // DEADLINE
        // ======================================

        public double DaysLeft =>
            (
                Goal.DeadLine.Date
                -
                DateTime.Today
            )
            .TotalDays;



        // ======================================
        // WEEKLY REQUIREMENT
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
                    DaysLeft /
                    7.0;



                if (weeksLeft <= 0)
                {
                    return 0;
                }



                return
                    Remaining /
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
            Remaining
                .ToString("C");



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



                return
                    $"{Math.Max(
                        (int)Math.Ceiling(
                            DaysLeft),
                        0)} days";
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
        // OPTIONAL EXTRA RECOMMENDATION
        // ======================================

        public double RecommendedAmount =>
            allocation?
                .RecommendedAmount
            ??
            0;


        public double RecommendedPercentage =>
            allocation?
                .RecommendedPercentage
            ??
            0;



        public string RecommendationHeaderText =>
            isCustomPreview
                ? "OPTIONAL EXTRA PREVIEW"
                : "POCKETAI OPTIONAL EXTRA";



        public string RecommendedAmountText
        {
            get
            {
                if (IsCompleted)
                {
                    return
                        "Goal complete";
                }



                if (optionalExtraSavings <= 0)
                {
                    return
                        "$0.00 extra this month";
                }



                return
                    $"{RecommendedAmount:C} extra this month";
            }
        }



        public string RecommendedPercentageText
        {
            get
            {
                if (IsCompleted)
                {
                    return
                        "";
                }



                if (optionalExtraSavings <= 0)
                {
                    return
                        "0% of optional extra";
                }



                return
                    $"{RecommendedPercentage:F0}% of optional extra";
            }
        }



        public string RecommendationReason
        {
            get
            {
                if (IsCompleted)
                {
                    return
                        "This goal is already fully funded.";
                }



                if (optionalExtraSavings <= 0)
                {
                    return
                        recommendationUnavailableReason;
                }



                if (isCustomPreview)
                {
                    return
                        "This is a preview only. PocketAI is dividing the optional extra amount you chose using the goal's Priority and Essential status. It has not reduced Safe to Spend.";
                }



                if (Goal.PriorityRank == 1 &&
                    Goal.IsEssential)
                {
                    return
                        "This optional extra receives additional weight because the goal is Priority 1 and Essential.";
                }



                if (Goal.IsEssential &&
                    Goal.PriorityRank > 0)
                {
                    return
                        $"This optional extra receives additional protection because the goal is Priority {Goal.PriorityRank} and Essential.";
                }



                if (Goal.PriorityRank == 1)
                {
                    return
                        "This goal receives a larger share of optional extra savings because it is Priority 1.";
                }



                if (Goal.PriorityRank > 0)
                {
                    return
                        $"This optional recommendation reflects the goal's Priority {Goal.PriorityRank} level.";
                }



                return
                    "This goal is unranked, so PocketAI treats it as a lower-priority destination for optional extra savings.";
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
                        "✓ GOAL COMPLETED";
                }


                if (DaysLeft <= 0)
                {
                    return
                        "⚠ PAST DUE";
                }


                return
                    "GOAL IN PROGRESS";
            }
        }



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
            double optionalExtraSavings,
            bool isCustomPreview,
            string recommendationUnavailableReason)
        {
            Goal =
                goal;


            this.allocation =
                allocation;


            this.optionalExtraSavings =
                optionalExtraSavings;


            this.isCustomPreview =
                isCustomPreview;


            this.recommendationUnavailableReason =
                recommendationUnavailableReason;
        }
    }
}