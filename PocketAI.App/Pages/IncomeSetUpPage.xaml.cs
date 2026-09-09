namespace PocketAI.App.Pages;


public partial class IncomeSetupPage : ContentPage
{
    private readonly DataBaseManager
        dataBaseManager;


    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public IncomeSetupPage()
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
    // CONTINUE
    // ==========================================

    private async void ContinueClicked(
        object? sender,
        EventArgs e)
    {
        string source =
            IncomeSourceEntry.Text?
                .Trim()
            ?? "";


        string incomeText =
            MonthlyIncomeEntry.Text?
                .Trim()
            ?? "";


        // ======================================
        // VALIDATE INCOME
        // ======================================

        if (!double.TryParse(
                incomeText,
                out double monthlyIncome)
            ||
            monthlyIncome <= 0)
        {
            await DisplayAlertAsync(
                "Invalid Income",
                "Enter your expected monthly income.",
                "OK");


            return;
        }


        // Source is useful, but we do not need
        // to block onboarding if the user does
        // not know what to call it.

        if (string.IsNullOrWhiteSpace(
                source))
        {
            source =
                "Primary Income";
        }


        // ======================================
        // SAVE USING EXISTING INCOME SYSTEM
        // ======================================

        Income income =
            new Income(
                source,
                monthlyIncome);


        dataBaseManager.SaveIncome(
            income);


        // Navigation to Account Setup will be
        // added after that page is built.

        await Navigation.PushAsync(
            new AccountSetupPage());
    }
}