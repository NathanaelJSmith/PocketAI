namespace PocketAI.App.Pages;

public partial class AccountsPage : ContentPage
{
    private readonly DataBaseManager dataBaseManager;


    public AccountsPage()
    {
        InitializeComponent();


        // Stores the database in PocketAI's
        // app data folder.
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


        // Refresh balances and income every time
        // the user returns to Accounts.
        LoadAccountData();
    }



    // ==========================================
    // LOAD ACCOUNT DATA
    // ==========================================

    private void LoadAccountData()
    {
        // ======================================
        // ACCOUNT BALANCES
        // ======================================

        AccountBalance? accountBalance =
            dataBaseManager
                .GetAccountBalance();


        if (accountBalance != null)
        {
            CheckingBalanceLabel.Text =
                accountBalance
                    .CheckingBalance
                    .ToString("C");


            SavingsBalanceLabel.Text =
                accountBalance
                    .SavingsBalance
                    .ToString("C");


            CashBalanceLabel.Text =
                accountBalance
                    .CashBalance
                    .ToString("C");


            TotalBalanceLabel.Text =
                accountBalance
                    .GetTotalBalance()
                    .ToString("C");
        }

        else
        {
            CheckingBalanceLabel.Text =
                "$0.00";


            SavingsBalanceLabel.Text =
                "$0.00";


            CashBalanceLabel.Text =
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


        if (income != null)
        {
            MonthlyIncomeLabel.Text =
                income
                    .MonthlyAmount
                    .ToString("C");
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


        double currentCash =
            currentBalance?
                .CashBalance
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
                        .ToString("0.00"));



        if (input == null)
        {
            return;
        }



        if (!double.TryParse(
                input,
                out double newChecking))
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Enter a valid checking balance.",
                "OK");


            return;
        }



        // Keep Savings and Cash unchanged.
        AccountBalance updatedBalance =
            new AccountBalance(
                newChecking,
                currentSavings,
                currentCash);



        dataBaseManager
            .SaveAccountBalance(
                updatedBalance);



        LoadAccountData();
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


        double currentCash =
            currentBalance?
                .CashBalance
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
                        .ToString("0.00"));



        if (input == null)
        {
            return;
        }



        if (!double.TryParse(
                input,
                out double newSavings)
            ||
            newSavings < 0)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Enter a valid savings balance.",
                "OK");


            return;
        }



        // Keep Checking and Cash unchanged.
        AccountBalance updatedBalance =
            new AccountBalance(
                currentChecking,
                newSavings,
                currentCash);



        dataBaseManager
            .SaveAccountBalance(
                updatedBalance);



        LoadAccountData();
    }



    // ==========================================
    // EDIT CASH
    // ==========================================

    private async void EditCashClicked(
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


        double currentCash =
            currentBalance?
                .CashBalance
            ??
            0;



        string? input =
            await DisplayPromptAsync(
                title:
                    "Edit Cash",

                message:
                    "Enter how much physical cash you currently have:",

                accept:
                    "Save",

                cancel:
                    "Cancel",

                keyboard:
                    Keyboard.Numeric,

                initialValue:
                    currentCash
                        .ToString("0.00"));



        if (input == null)
        {
            return;
        }



        if (!double.TryParse(
                input,
                out double newCash)
            ||
            newCash < 0)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Enter a valid cash balance.",
                "OK");


            return;
        }



        // Keep Checking and Savings unchanged.
        AccountBalance updatedBalance =
            new AccountBalance(
                currentChecking,
                currentSavings,
                newCash);



        dataBaseManager
            .SaveAccountBalance(
                updatedBalance);



        LoadAccountData();
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
                        .ToString("0.00"));



        if (incomeInput == null)
        {
            return;
        }



        if (!double.TryParse(
                incomeInput,
                out double monthlyIncome)
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



        dataBaseManager
            .SaveIncome(
                income);



        LoadAccountData();
    }
}