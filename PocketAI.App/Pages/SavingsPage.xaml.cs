namespace PocketAI.App.Pages;

public partial class SavingsPage : ContentPage
{
    private readonly DataBaseManager dataBaseManager;
    private readonly AnalyticsService analyticsService;

    // All savings goals stored in SQLite
    private List<SavingsGoal> savingsGoals =
        new List<SavingsGoal>();

    // Goal currently being edited or updated
    private SavingsGoal? selectedGoal;


    public SavingsPage()
    {
        InitializeComponent();

        string databasePath =
            Path.Combine(
                FileSystem.AppDataDirectory,
                "pocketai.db");

        dataBaseManager =
            new DataBaseManager(databasePath);

        analyticsService =
            new AnalyticsService();

        dataBaseManager.CreateTables();
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadSavingsGoals();
    }



    // ==========================================
    // LOAD ALL SAVINGS GOALS
    // ==========================================

    private void LoadSavingsGoals()
    {
        savingsGoals =
            dataBaseManager
                .GetSavingsGoals();


        List<SavingsGoalDisplayItem> displayItems =
            savingsGoals
                .Select(goal =>
                    new SavingsGoalDisplayItem(
                        goal,
                        analyticsService))
                .ToList();


        BindableLayout.SetItemsSource(
            SavingsGoalsContainer,
            displayItems);


        bool hasGoals =
            savingsGoals.Count > 0;


        SavingsEmptyState.IsVisible =
            !hasGoals;


        // ======================================
        // SUMMARY
        // ======================================

        double totalSaved =
            savingsGoals.Sum(
                goal => goal.CurrentAmount);


        double totalTarget =
            savingsGoals.Sum(
                goal => goal.TargetAmount);


        double totalRemaining =
            savingsGoals.Sum(
                goal =>
                    Math.Max(
                        goal.TargetAmount -
                        goal.CurrentAmount,
                        0));


        TotalSavedLabel.Text =
            totalSaved.ToString("C");


        TotalTargetLabel.Text =
            totalTarget.ToString("C");


        TotalRemainingLabel.Text =
            totalRemaining.ToString("C");


        GoalCountLabel.Text =
            savingsGoals.Count.ToString();
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
            DateTime.Today.AddMonths(6);


        AddSavingsModal.IsVisible =
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


        AddSavingsModal.IsVisible =
            false;


        ModalBackground.IsVisible =
            true;


        GoalModal.IsVisible =
            true;
    }



    // ==========================================
    // SAVE NEW / EDITED GOAL
    // ==========================================

    private async void SaveGoalClicked(
        object? sender,
        EventArgs e)
    {
        string name =
            GoalNameEntry.Text?
                .Trim() ?? "";


        string targetText =
            GoalTargetEntry.Text?
                .Trim() ?? "";


        string currentText =
            GoalCurrentEntry.Text?
                .Trim() ?? "";


        // --------------------------------------
        // NAME VALIDATION
        // --------------------------------------

        if (string.IsNullOrWhiteSpace(
                name))
        {
            await DisplayAlertAsync(
                "Missing Name",
                "Enter a savings goal name.",
                "OK");

            return;
        }


        // --------------------------------------
        // TARGET VALIDATION
        // --------------------------------------

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


        // --------------------------------------
        // CURRENT SAVINGS VALIDATION
        // --------------------------------------

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
            ?? DateTime.Today.AddMonths(6);


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


            dataBaseManager.AddSavingsGoal(
                newGoal);
        }

        // ======================================
        // EDIT EXISTING GOAL
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
                    selectedGoal.IsPrimary);


            dataBaseManager.UpdateSavingsGoal(
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
                .Trim() ?? "";


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
            + amount;


        SavingsGoal updatedGoal =
            new SavingsGoal(
                selectedGoal.Id,
                selectedGoal.Name,
                selectedGoal.TargetAmount,
                newCurrentAmount,
                selectedGoal.DeadLine,
                selectedGoal.IsPrimary);


        dataBaseManager.UpdateSavingsGoal(
            updatedGoal);


        CloseSavingsModals();

        LoadSavingsGoals();
    }



    // ==========================================
    // MAKE PRIMARY
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
    // CANCEL
    // ==========================================

    private void CancelSavingsModalClicked(
        object? sender,
        EventArgs e)
    {
        CloseSavingsModals();
    }



    // ==========================================
    // CLICK DARK BACKGROUND
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


        ModalBackground.IsVisible =
            false;


        selectedGoal =
            null;
    }



    // ==========================================
    // SAVINGS GOAL DISPLAY MODEL
    // ==========================================

    public class SavingsGoalDisplayItem
    {
        public SavingsGoal Goal { get; }


        public string Name =>
            Goal.Name;


        public bool IsPrimary =>
            Goal.IsPrimary;


        public bool CanMakePrimary =>
            !Goal.IsPrimary;


        public double Remaining =>
            Math.Max(
                Goal.TargetAmount -
                Goal.CurrentAmount,
                0);


        public double Percent
        {
            get
            {
                if (Goal.TargetAmount <= 0)
                {
                    return 0;
                }


                return Math.Clamp(
                    (Goal.CurrentAmount /
                     Goal.TargetAmount)
                    * 100,
                    0,
                    100);
            }
        }


        public double Progress =>
            Percent / 100.0;


        public double DaysLeft =>
            (Goal.DeadLine.Date -
             DateTime.Today).TotalDays;


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
                    DaysLeft / 7.0;


                if (weeksLeft <= 0)
                {
                    return 0;
                }


                return Remaining /
                       weeksLeft;
            }
        }


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
                if (Remaining <= 0)
                {
                    return "Complete";
                }


                int days =
                    Math.Max(
                        (int)Math.Ceiling(
                            DaysLeft),
                        0);


                return $"{days} days";
            }
        }


        public string WeeklyNeededText =>
            WeeklyNeeded.ToString("C");


        public string StatusText
        {
            get
            {
                if (Remaining <= 0)
                {
                    return "✓ Goal complete";
                }


                if (DaysLeft <= 0)
                {
                    return
                        $"⚠ Target date reached with {Remaining:C} remaining";
                }


                return
                    $"Save about {WeeklyNeeded:C} per week to stay on track.";
            }
        }


        public Color StatusColor
        {
            get
            {
                if (Remaining <= 0)
                {
                    return Color.FromArgb(
                        "#15803D");
                }


                if (DaysLeft <= 0)
                {
                    return Color.FromArgb(
                        "#B91C1C");
                }


                return Color.FromArgb(
                    "#15803D");
            }
        }


        public SavingsGoalDisplayItem(
            SavingsGoal goal,
            AnalyticsService analyticsService)
        {
            Goal =
                goal;
        }
    }
}