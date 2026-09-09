namespace PocketAI.App.Pages;


public partial class AccountSetupPage : ContentPage
{
    private readonly DataBaseManager
        dataBaseManager;


    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public AccountSetupPage()
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


        LoadExistingBalances();
    }


    // ==========================================
    // LOAD EXISTING BALANCES
    // ==========================================

    private void LoadExistingBalances()
    {
        AccountBalance? accountBalance =
            dataBaseManager
                .GetAccountBalance();


        if (accountBalance == null)
        {
            return;
        }


        CheckingBalanceEntry.Text =
            accountBalance
                .CheckingBalance
                .ToString("0.00");


        SavingsBalanceEntry.Text =
            accountBalance
                .SavingsBalance
                .ToString("0.00");


        CashBalanceEntry.Text =
            accountBalance
                .CashBalance
                .ToString("0.00");
    }


    // ==========================================
    // CONTINUE
    // ==========================================

    private async void ContinueClicked(
        object? sender,
        EventArgs e)
    {
        if (!TryReadAmount(
                CheckingBalanceEntry.Text,
                out double checking))
        {
            await DisplayAlertAsync(
                "Invalid Checking Balance",
                "Enter a valid checking balance.",
                "OK");


            return;
        }


        if (!TryReadAmount(
                SavingsBalanceEntry.Text,
                out double savings))
        {
            await DisplayAlertAsync(
                "Invalid Savings Balance",
                "Enter a valid savings balance.",
                "OK");


            return;
        }


        if (!TryReadAmount(
                CashBalanceEntry.Text,
                out double cash))
        {
            await DisplayAlertAsync(
                "Invalid Cash Balance",
                "Enter a valid cash balance.",
                "OK");


            return;
        }


        AccountBalance accountBalance =
            new AccountBalance(
                checking,
                savings,
                cash);


        dataBaseManager
            .SaveAccountBalance(
                accountBalance);


        await Navigation.PushAsync(
            new BillsSetupPage());


        // Recurring Bills setup will be
        // connected in a later step.
    }


    // ==========================================
    // READ AMOUNT
    // ==========================================
    //
    // Leaving an optional account blank means
    // the user currently has $0 there.
    // ==========================================

    private bool TryReadAmount(
        string? text,
        out double amount)
    {
        if (string.IsNullOrWhiteSpace(
                text))
        {
            amount = 0;

            return true;
        }


        return double.TryParse(
            text.Trim(),
            out amount);
    }
}