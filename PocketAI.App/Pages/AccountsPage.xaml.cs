namespace PocketAI.App.Pages;

public partial class AccountsPage : ContentPage
{
    private readonly DataBaseManager dataBaseManager;

    public AccountsPage()
    {
        InitializeComponent();

        // Stores the database in PocketAI's app data folder
        string databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "pocketai.db");

        dataBaseManager =
            new DataBaseManager(databasePath);

        dataBaseManager.CreateTables();
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Refreshes balances and income every time
        // the user returns to the Accounts page
        LoadAccountData();
    }


    private void LoadAccountData()
    {
        // ==========================================
        // LOAD ACCOUNT BALANCES
        // ==========================================

        AccountBalance? accountBalance =
            dataBaseManager.GetAccountBalance();

        if (accountBalance != null)
        {
            CheckingBalanceLabel.Text =
                accountBalance.CheckingBalance.ToString("C");

            SavingsBalanceLabel.Text =
                accountBalance.SavingsBalance.ToString("C");

            CashBalanceLabel.Text =
                accountBalance.CashBalance.ToString("C");

            TotalBalanceLabel.Text =
                accountBalance.GetTotalBalance().ToString("C");
        }
        else
        {
            CheckingBalanceLabel.Text = "$0.00";
            SavingsBalanceLabel.Text = "$0.00";
            CashBalanceLabel.Text = "$0.00";
            TotalBalanceLabel.Text = "$0.00";
        }


        // ==========================================
        // LOAD MONTHLY INCOME
        // ==========================================

        Income? income =
            dataBaseManager.GetIncome();

        if (income != null)
        {
            MonthlyIncomeLabel.Text =
                income.MonthlyAmount.ToString("C");
        }
        else
        {
            MonthlyIncomeLabel.Text = "$0.00";
        }
    }


    private async void UpdateBalancesClicked(
        object? sender,
        EventArgs e)
    {
        // Ask for checking balance
        string? checkingInput =
            await DisplayPromptAsync(
                "Checking Balance",
                "Enter your current checking balance:");

        if (checkingInput == null)
        {
            return;
        }


        // Ask for savings balance
        string? savingsInput =
            await DisplayPromptAsync(
                "Savings Balance",
                "Enter your current savings balance:");

        if (savingsInput == null)
        {
            return;
        }


        // Ask for cash balance
        string? cashInput =
            await DisplayPromptAsync(
                "Cash Balance",
                "Enter your current cash balance:");

        if (cashInput == null)
        {
            return;
        }


        // Converts the entered values into numbers
        if (!double.TryParse(checkingInput, out double checking) ||
            !double.TryParse(savingsInput, out double savings) ||
            !double.TryParse(cashInput, out double cash))
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Please enter valid numbers for each balance.",
                "OK");

            return;
        }


        AccountBalance accountBalance =
        new AccountBalance(
        checking,
        savings,
        cash);


        // Saves the balances to SQLite
        dataBaseManager.SaveAccountBalance(
            accountBalance);


        // Refresh the page
        LoadAccountData();
    }


    private async void UpdateIncomeClicked(
        object? sender,
        EventArgs e)
    {
        string? incomeInput =
            await DisplayPromptAsync(
                "Monthly Income",
                "Enter your monthly income:");

        if (incomeInput == null)
        {
            return;
        }


        if (!double.TryParse(
                incomeInput,
                out double monthlyIncome))
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


        // Saves income to SQLite
        dataBaseManager.SaveIncome(income);


        // Refresh the page
        LoadAccountData();
    }
}