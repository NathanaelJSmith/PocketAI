using System.Linq;

namespace PocketAI.App.Pages;


public partial class SavingsSetupPage : ContentPage
{
    private readonly DataBaseManager
        dataBaseManager;


    public SavingsSetupPage()
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


        GoalDeadlinePicker.Date =
            DateTime.Today
                .AddMonths(6);
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();


        LoadSavingsSummary();
    }


    // ==========================================
    // LOAD SAVINGS SUMMARY
    // ==========================================

    private void LoadSavingsSummary()
    {
        AccountBalance? accountBalance =
            dataBaseManager
                .GetAccountBalance();


        List<SavingsGoal> goals =
            dataBaseManager
                .GetSavingsGoals();


        double savingsBalance =
            Math.Max(
                accountBalance?
                    .SavingsBalance
                ??
                0,
                0);


        double assignedSavings =
            goals.Sum(
                goal =>
                    Math.Max(
                        goal.CurrentAmount,
                        0));


        double unassignedSavings =
            Math.Max(
                savingsBalance
                -
                assignedSavings,
                0);


        int activeGoals =
            goals.Count(
                goal =>
                    !goal.IsCompleted);


        SavingsBalanceLabel.Text =
            savingsBalance.ToString("C");


        AssignedSavingsLabel.Text =
            assignedSavings.ToString("C");


        UnassignedSavingsLabel.Text =
            unassignedSavings.ToString("C");


        GoalCountLabel.Text =
            activeGoals.ToString();
    }


    // ==========================================
    // ADD GOAL
    // ==========================================

    private async void AddGoalClicked(
        object? sender,
        EventArgs e)
    {
        string name =
            GoalNameEntry.Text?
                .Trim()
            ?? "";


        string targetText =
            GoalTargetEntry.Text?
                .Trim()
            ?? "";


        string currentText =
            GoalCurrentEntry.Text?
                .Trim()
            ?? "";


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


        List<SavingsGoal> existingGoals =
            dataBaseManager
                .GetSavingsGoals();


        AccountBalance? accountBalance =
            dataBaseManager
                .GetAccountBalance();


        double savingsBalance =
            Math.Max(
                accountBalance?
                    .SavingsBalance
                ??
                0,
                0);


        double assignedSavings =
            existingGoals.Sum(
                goal =>
                    Math.Max(
                        goal.CurrentAmount,
                        0));


        double unassignedSavings =
            Math.Max(
                savingsBalance
                -
                assignedSavings,
                0);


        if (currentAmount >
            unassignedSavings)
        {
            await DisplayAlertAsync(
                "Not Enough Unassigned Savings",
                $"You only have {unassignedSavings:C} available to assign. Savings goals organize money already in your Savings Account.",
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


        int nextPriority =
            existingGoals
                .Where(
                    goal =>
                        !goal.IsCompleted
                        &&
                        goal.PriorityRank > 0)
                .Select(
                    goal =>
                        goal.PriorityRank)
                .DefaultIfEmpty(0)
                .Max()
            +
            1;


        SavingsGoal newGoal =
            new SavingsGoal(
                name,
                targetAmount,
                currentAmount,
                deadline);


        newGoal.PriorityRank =
            nextPriority;


        newGoal.IsEssential =
            false;


        newGoal.CustomAllocationPercentage =
            null;


        dataBaseManager
            .AddSavingsGoal(
                newGoal);


        ClearGoalForm();


        LoadSavingsSummary();


        await DisplayAlertAsync(
            "Goal Added",
            $"{name} is now part of your savings plan.",
            "OK");
    }


    // ==========================================
    // CLEAR FORM
    // ==========================================

    private void ClearGoalForm()
    {
        GoalNameEntry.Text =
            "";


        GoalTargetEntry.Text =
            "";


        GoalCurrentEntry.Text =
            "0";


        GoalDeadlinePicker.Date =
            DateTime.Today
                .AddMonths(6);
    }


    // ==========================================
    // CONTINUE
    // ==========================================

    private async void ContinueClicked(
        object? sender,
        EventArgs e)
    {
        await Navigation.PushAsync(
        new MeetPocketAIPage());
    }
}