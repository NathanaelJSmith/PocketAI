namespace PocketAI.App.Pages;


public partial class AccountSetupPage : ContentPage
{
    private readonly DataBaseManager
        dataBaseManager;


    private bool isSaving =
        false;



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


        if (accountBalance ==
            null)
        {
            CheckingBalanceEntry.Text =
                "";


            SavingsBalanceEntry.Text =
                "";


            return;
        }


        CheckingBalanceEntry.Text =
            accountBalance
                .CheckingBalance
                .ToString(
                    "0.00");


        SavingsBalanceEntry.Text =
            accountBalance
                .SavingsBalance
                .ToString(
                    "0.00");
    }



    // ==========================================
    // CONTINUE
    // ==========================================

    private async void ContinueClicked(
        object? sender,
        EventArgs e)
    {
        if (isSaving)
        {
            return;
        }



        // ======================================
        // CHECKING
        // ======================================

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



        // ======================================
        // SAVINGS
        // ======================================

        if (!TryReadAmount(
                SavingsBalanceEntry.Text,
                out double savings)
            ||
            savings < 0)
        {
            await DisplayAlertAsync(
                "Invalid Savings Balance",
                "Enter a valid savings balance of zero or more.",
                "OK");


            return;
        }



        // ======================================
        // BUILD ACCOUNT BALANCE
        // ======================================
        //
        // Cash has been removed from PocketAI.
        //
        // The database still temporarily has an
        // old CashBalance field, so we explicitly
        // save zero for compatibility.
        // ======================================

        AccountBalance accountBalance =
            new AccountBalance(
                checking,
                savings,
                0);



        // ======================================
        // SAVE
        // ======================================

        try
        {
            isSaving =
                true;


            dataBaseManager
                .SaveAccountBalance(
                    accountBalance);


            await Navigation.PushAsync(
                new BillsSetupPage());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Failed to save onboarding account balances: {ex}");


            await DisplayAlertAsync(
                "Unable to Save",
                "PocketAI could not save your account balances. Please try again.",
                "OK");
        }
        finally
        {
            isSaving =
                false;
        }
    }



    // ==========================================
    // READ AMOUNT
    // ==========================================
    //
    // Leaving an account blank means $0.
    // ==========================================

    private bool TryReadAmount(
        string? text,
        out double amount)
    {
        if (string.IsNullOrWhiteSpace(
                text))
        {
            amount =
                0;


            return true;
        }


        if (!double.TryParse(
                text.Trim(),
                out amount))
        {
            return false;
        }


        if (!double.IsFinite(
                amount))
        {
            return false;
        }


        return true;
    }
}