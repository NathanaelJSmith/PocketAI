namespace PocketAI.App.Pages;


public partial class AccountsPage : ContentPage
{
    private readonly DataBaseManager
        dataBaseManager;



    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public AccountsPage()
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
    }



    // ==========================================
    // PAGE APPEARS
    // ==========================================

    protected override void OnAppearing()
    {
        base.OnAppearing();


        LoadAccountData();
    }



    // ==========================================
    // LOAD ACCOUNT DATA
    // ==========================================

    private void LoadAccountData()
    {
        AccountBalance? accountBalance =
            dataBaseManager
                .GetAccountBalance();



        // ======================================
        // ACCOUNT BALANCES
        // ======================================

        if (accountBalance !=
            null)
        {
            CheckingBalanceLabel.Text =
                accountBalance
                    .CheckingBalance
                    .ToString(
                        "C");


            SavingsBalanceLabel.Text =
                accountBalance
                    .SavingsBalance
                    .ToString(
                        "C");


            // Cash is no longer part of
            // PocketAI's displayed total.
            double totalBalance =
                accountBalance
                    .CheckingBalance
                +
                accountBalance
                    .SavingsBalance;


            TotalBalanceLabel.Text =
                totalBalance
                    .ToString(
                        "C");


            // ==================================
            // REMOVE LEGACY CASH VALUE
            // ==================================
            //
            // Existing alpha databases may still
            // contain an old physical-cash value.
            //
            // PocketAI no longer uses Cash, so
            // normalize it to zero.
            // ==================================

            if (Math.Abs(
                    accountBalance
                        .CashBalance)
                >
                0.001)
            {
                try
                {
                    AccountBalance cleanedBalance =
                        new AccountBalance(
                            accountBalance
                                .CheckingBalance,
                            accountBalance
                                .SavingsBalance,
                            0);


                    dataBaseManager
                        .SaveAccountBalance(
                            cleanedBalance);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Failed to clear legacy cash balance: {ex}");
                }
            }
        }

        else
        {
            CheckingBalanceLabel.Text =
                "$0.00";


            SavingsBalanceLabel.Text =
                "$0.00";


            TotalBalanceLabel.Text =
                "$0.00";
        }



        // ======================================
        // MONTHLY INCOME
        // ======================================

        Income? income =
            dataBaseManager
                .GetIncome();


        if (income !=
            null)
        {
            MonthlyIncomeLabel.Text =
                income
                    .MonthlyAmount
                    .ToString(
                        "C");
        }

        else
        {
            MonthlyIncomeLabel.Text =
                "$0.00";
        }
    }



    // ==========================================
    // EDIT CHECKING
    // ==========================================

    private async void EditCheckingClicked(
        object? sender,
        EventArgs e)
    {
        AccountBalance? currentBalance =
            dataBaseManager
                .GetAccountBalance();


        double currentChecking =
            currentBalance?
                .CheckingBalance
            ??
            0;


        double currentSavings =
            currentBalance?
                .SavingsBalance
            ??
            0;



        string? input =
            await DisplayPromptAsync(
                title:
                    "Edit Checking",

                message:
                    "Enter your current checking balance:",

                accept:
                    "Save",

                cancel:
                    "Cancel",

                keyboard:
                    Keyboard.Numeric,

                initialValue:
                    currentChecking
                        .ToString(
                            "0.00"));



        if (input ==
            null)
        {
            return;
        }



        if (!double.TryParse(
                input,
                out double newChecking)
            ||
            !double.IsFinite(
                newChecking))
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Enter a valid checking balance.",
                "OK");


            return;
        }



        AccountBalance updatedBalance =
            new AccountBalance(
                newChecking,
                currentSavings,
                0);



        try
        {
            dataBaseManager
                .SaveAccountBalance(
                    updatedBalance);


            LoadAccountData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Failed to save checking balance: {ex}");


            await DisplayAlertAsync(
                "Unable to Save",
                "PocketAI could not update your checking balance. Please try again.",
                "OK");
        }
    }



    // ==========================================
    // EDIT SAVINGS
    // ==========================================

    private async void EditSavingsClicked(
        object? sender,
        EventArgs e)
    {
        AccountBalance? currentBalance =
            dataBaseManager
                .GetAccountBalance();


        double currentChecking =
            currentBalance?
                .CheckingBalance
            ??
            0;


        double currentSavings =
            currentBalance?
                .SavingsBalance
            ??
            0;



        string? input =
            await DisplayPromptAsync(
                title:
                    "Edit Savings",

                message:
                    "Enter your current savings balance:",

                accept:
                    "Save",

                cancel:
                    "Cancel",

                keyboard:
                    Keyboard.Numeric,

                initialValue:
                    currentSavings
                        .ToString(
                            "0.00"));



        if (input ==
            null)
        {
            return;
        }



        if (!double.TryParse(
                input,
                out double newSavings)
            ||
            !double.IsFinite(
                newSavings)
            ||
            newSavings < 0)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Enter a valid savings balance.",
                "OK");


            return;
        }



        // ======================================
        // PROTECT SAVINGS GOAL ASSIGNMENTS
        // ======================================

        List<SavingsGoal> goals =
            dataBaseManager
                .GetSavingsGoals();


        double assignedSavings =
            goals.Sum(
                goal =>
                    Math.Max(
                        goal.CurrentAmount,
                        0));


        if (newSavings <
            assignedSavings)
        {
            await DisplayAlertAsync(
                "Savings Already Assigned",
                $"You currently have {assignedSavings:C} assigned to savings goals. Your Savings Account cannot be lowered below that amount.",
                "OK");


            return;
        }



        AccountBalance updatedBalance =
            new AccountBalance(
                currentChecking,
                newSavings,
                0);



        try
        {
            dataBaseManager
                .SaveAccountBalance(
                    updatedBalance);


            LoadAccountData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Failed to save savings balance: {ex}");


            await DisplayAlertAsync(
                "Unable to Save",
                "PocketAI could not update your savings balance. Please try again.",
                "OK");
        }
    }



    // ==========================================
    // UPDATE MONTHLY INCOME
    // ==========================================

    private async void UpdateIncomeClicked(
        object? sender,
        EventArgs e)
    {
        Income? currentIncome =
            dataBaseManager
                .GetIncome();


        double existingIncome =
            currentIncome?
                .MonthlyAmount
            ??
            0;



        string? incomeInput =
            await DisplayPromptAsync(
                title:
                    "Monthly Income",

                message:
                    "Enter your expected monthly income:",

                accept:
                    "Save",

                cancel:
                    "Cancel",

                keyboard:
                    Keyboard.Numeric,

                initialValue:
                    existingIncome
                        .ToString(
                            "0.00"));



        if (incomeInput ==
            null)
        {
            return;
        }



        if (!double.TryParse(
                incomeInput,
                out double monthlyIncome)
            ||
            !double.IsFinite(
                monthlyIncome)
            ||
            monthlyIncome < 0)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Please enter a valid monthly income.",
                "OK");


            return;
        }



        Income income =
            new Income(
                "Monthly Income",
                monthlyIncome);



        try
        {
            dataBaseManager
                .SaveIncome(
                    income);


            LoadAccountData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Failed to save income: {ex}");


            await DisplayAlertAsync(
                "Unable to Save",
                "PocketAI could not update your monthly income. Please try again.",
                "OK");
        }
    }
}