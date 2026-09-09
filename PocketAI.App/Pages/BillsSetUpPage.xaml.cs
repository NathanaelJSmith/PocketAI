namespace PocketAI.App.Pages;


public partial class BillsSetupPage : ContentPage
{
    private readonly DataBaseManager
        dataBaseManager;


    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public BillsSetupPage()
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


        SetupCategories();
    }


    // ==========================================
    // PAGE APPEARS
    // ==========================================

    protected override void OnAppearing()
    {
        base.OnAppearing();


        LoadBillSummary();
    }


    // ==========================================
    // CATEGORIES
    // ==========================================

    private void SetupCategories()
    {
        BillCategoryPicker.ItemsSource =
            new List<string>
            {
                "Housing",
                "Utilities",
                "Subscriptions",
                "Insurance",
                "Transportation",
                "Debt",
                "Health",
                "Education",
                "Entertainment",
                "Other"
            };
    }


    // ==========================================
    // LOAD CURRENT BILL SUMMARY
    // ==========================================

    private void LoadBillSummary()
    {
        List<RecurringExpenses> bills =
            dataBaseManager
                .GetRecuringExpenses();


        List<RecurringExpenses> activeBills =
            bills
                .Where(
                    bill =>
                        bill.IsActive)
                .ToList();


        double monthlyTotal =
            activeBills.Sum(
                bill =>
                    bill.Amount);


        BillCountLabel.Text =
            activeBills.Count == 1
                ? "1 active bill"
                : $"{activeBills.Count} active bills";


        MonthlyBillsTotalLabel.Text =
            monthlyTotal.ToString("C");
    }


    // ==========================================
    // ADD BILL
    // ==========================================

    private async void AddBillClicked(
        object? sender,
        EventArgs e)
    {
        string name =
            BillNameEntry.Text?
                .Trim()
            ?? "";


        string category =
            BillCategoryPicker
                .SelectedItem?
                .ToString()
            ?? "";


        string amountText =
            BillAmountEntry.Text?
                .Trim()
            ?? "";


        string dueDayText =
            BillDueDayEntry.Text?
                .Trim()
            ?? "";


        if (string.IsNullOrWhiteSpace(
                name))
        {
            await DisplayAlertAsync(
                "Missing Name",
                "Enter a name for the bill.",
                "OK");


            return;
        }


        if (string.IsNullOrWhiteSpace(
                category))
        {
            await DisplayAlertAsync(
                "Missing Category",
                "Select a category.",
                "OK");


            return;
        }


        if (!double.TryParse(
                amountText,
                out double amount)
            ||
            amount <= 0)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Enter a valid monthly amount.",
                "OK");


            return;
        }


        if (!int.TryParse(
                dueDayText,
                out int dueDay)
            ||
            dueDay < 1
            ||
            dueDay > 31)
        {
            await DisplayAlertAsync(
                "Invalid Due Day",
                "Enter a day between 1 and 31.",
                "OK");


            return;
        }


        RecurringExpenses newBill =
            new RecurringExpenses(
                0,
                name,
                category,
                amount,
                dueDay,
                true);


        dataBaseManager
            .AddRecurringExpense(
                newBill);


        ClearBillForm();


        LoadBillSummary();


        await DisplayAlertAsync(
            "Bill Added",
            $"{name} has been added to your monthly plan.",
            "OK");
    }


    // ==========================================
    // CLEAR FORM
    // ==========================================

    private void ClearBillForm()
    {
        BillNameEntry.Text =
            "";


        BillCategoryPicker.SelectedIndex =
            -1;


        BillAmountEntry.Text =
            "";


        BillDueDayEntry.Text =
            "";
    }


    // ==========================================
    // CONTINUE
    // ==========================================

    private async void ContinueClicked(
        object? sender,
        EventArgs e)
    {
        await Navigation.PushAsync(
        new SavingsSetupPage());
    }
}