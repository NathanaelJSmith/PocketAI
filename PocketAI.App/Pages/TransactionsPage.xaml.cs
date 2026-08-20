namespace PocketAI.App.Pages;

public partial class TransactionsPage : ContentPage
{
    private readonly DataBaseManager dataBaseManager;

    // Stores every expense loaded from SQLite
    private List<Expense> allExpenses = new List<Expense>();

    // Stores the expense currently being edited
    private Expense? selectedExpense;


    public TransactionsPage()
    {
        InitializeComponent();

        // Use the same MAUI database as the other pages
        string databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "pocketai.db");

        dataBaseManager =
            new DataBaseManager(databasePath);

        dataBaseManager.CreateTables();

        SetupCategories();
        SetupMonths();
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Refresh every time the user returns
        LoadTransactions();
    }



    // ==========================================
    // CATEGORIES
    // ==========================================

    private void SetupCategories()
    {
        List<string> categories = new List<string>
        {
            "All Categories",
            "Dining",
            "Groceries",
            "Gas",
            "Entertainment",
            "Shopping",
            "Transportation",
            "Housing",
            "Utilities",
            "Health",
            "Education",
            "Other"
        };


        // Main transaction filter
        CategoryPicker.ItemsSource =
            categories;


        // Add Expense picker
        ExpenseCategoryPicker.ItemsSource =
            categories.Skip(1).ToList();


        // Edit Expense picker
        EditExpenseCategoryPicker.ItemsSource =
            categories.Skip(1).ToList();


        CategoryPicker.SelectedIndex = 0;
    }



    // ==========================================
    // MONTH FILTER
    // ==========================================

    private void SetupMonths()
    {
        List<string> months =
            new List<string>
            {
                "All Months"
            };


        DateTime currentMonth =
            new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);


        // Add the most recent 12 months
        for (int i = 0; i < 12; i++)
        {
            months.Add(
                currentMonth
                    .AddMonths(-i)
                    .ToString("MMMM yyyy"));
        }


        MonthPicker.ItemsSource =
            months;


        // Default to current month
        MonthPicker.SelectedIndex = 1;
    }



    // ==========================================
    // LOAD TRANSACTIONS
    // ==========================================

    private void LoadTransactions()
    {
        allExpenses =
            dataBaseManager
                .GetAllExpenses()
                .OrderByDescending(expense =>
                    expense.Date)
                .ThenByDescending(expense =>
                    expense.Id)
                .ToList();


        ApplyFilters();


        // Calculate total spent this month
        DateTime today =
            DateTime.Today;


        double thisMonthTotal =
            allExpenses
                .Where(expense =>
                    expense.Date.Year == today.Year &&
                    expense.Date.Month == today.Month)
                .Sum(expense =>
                    expense.Amount);


        ThisMonthTotalLabel.Text =
            thisMonthTotal.ToString("C");
    }



    // ==========================================
    // SEARCH
    // ==========================================

    private void TransactionSearchChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        ApplyFilters();
    }



    // ==========================================
    // FILTER
    // ==========================================

    private void FilterChanged(
        object? sender,
        EventArgs e)
    {
        ApplyFilters();
    }



    // ==========================================
    // APPLY SEARCH + FILTERS
    // ==========================================

    private void ApplyFilters()
    {
        IEnumerable<Expense> filteredExpenses =
            allExpenses;


        // --------------------------------------
        // SEARCH
        // --------------------------------------

        string searchText =
            TransactionSearchBar.Text?.Trim() ?? "";


        if (!string.IsNullOrWhiteSpace(
                searchText))
        {
            filteredExpenses =
                filteredExpenses.Where(
                    expense =>
                        expense.Name.Contains(
                            searchText,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        expense.Category.Contains(
                            searchText,
                            StringComparison.OrdinalIgnoreCase));
        }


        // --------------------------------------
        // CATEGORY FILTER
        // --------------------------------------

        if (CategoryPicker.SelectedIndex > 0)
        {
            string selectedCategory =
                CategoryPicker
                    .SelectedItem?
                    .ToString() ?? "";


            filteredExpenses =
                filteredExpenses.Where(
                    expense =>
                        expense.Category.Equals(
                            selectedCategory,
                            StringComparison.OrdinalIgnoreCase));
        }


        // --------------------------------------
        // MONTH FILTER
        // --------------------------------------

        if (MonthPicker.SelectedIndex > 0)
        {
            string selectedMonth =
                MonthPicker
                    .SelectedItem?
                    .ToString() ?? "";


            if (DateTime.TryParse(
                    $"1 {selectedMonth}",
                    out DateTime monthDate))
            {
                filteredExpenses =
                    filteredExpenses.Where(
                        expense =>
                            expense.Date.Year ==
                            monthDate.Year
                            &&
                            expense.Date.Month ==
                            monthDate.Month);
            }
        }


        // Convert expenses into display items
        List<TransactionDisplayItem> displayItems =
            filteredExpenses
                .Select(expense =>
                    new TransactionDisplayItem(
                        expense))
                .ToList();


        TransactionsCollectionView.ItemsSource =
            displayItems;


        NoTransactionsLabel.IsVisible =
            displayItems.Count == 0;


        TransactionsCollectionView.IsVisible =
            displayItems.Count > 0;
    }



    // ==========================================
    // SHOW ADD EXPENSE MODAL
    // ==========================================

    private void ShowAddExpenseClicked(
        object? sender,
        EventArgs e)
    {
        selectedExpense = null;


        // Clear old values
        ExpenseNameEntry.Text = "";
        ExpenseAmountEntry.Text = "";

        ExpenseCategoryPicker.SelectedIndex =
            -1;

        ExpenseDatePicker.Date =
            DateTime.Today;


        // Make sure edit modal is closed
        EditExpenseModal.IsVisible =
            false;


        // Show Add Expense
        ModalBackground.IsVisible =
            true;

        AddExpenseModal.IsVisible =
            true;
    }



    // ==========================================
    // CANCEL ADD EXPENSE
    // ==========================================

    private void CancelAddExpenseClicked(
        object? sender,
        EventArgs e)
    {
        CloseModals();
    }



    // ==========================================
    // CLOSE MODALS BY CLICKING BACKGROUND
    // ==========================================

    private void CloseModalsClicked(
        object? sender,
        TappedEventArgs e)
    {
        CloseModals();
    }



    // ==========================================
    // CLOSE ALL MODALS
    // ==========================================

    private void CloseModals()
    {
        AddExpenseModal.IsVisible =
            false;

        EditExpenseModal.IsVisible =
            false;

        ModalBackground.IsVisible =
            false;

        selectedExpense =
            null;
    }



    // ==========================================
    // ADD EXPENSE
    // ==========================================

    private async void AddExpenseClicked(
        object? sender,
        EventArgs e)
    {
        string expenseName =
            ExpenseNameEntry.Text?.Trim() ?? "";


        string amountText =
            ExpenseAmountEntry.Text?.Trim() ?? "";


        string category =
            ExpenseCategoryPicker
                .SelectedItem?
                .ToString() ?? "";


        // Validate name
        if (string.IsNullOrWhiteSpace(
                expenseName))
        {
            await DisplayAlertAsync(
                "Missing Name",
                "Enter a name for the expense.",
                "OK");

            return;
        }


        // Validate amount
        if (!double.TryParse(
                amountText,
                out double amount)
            ||
            amount <= 0)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Enter a valid expense amount.",
                "OK");

            return;
        }


        // Validate category
        if (string.IsNullOrWhiteSpace(
                category))
        {
            await DisplayAlertAsync(
                "Missing Category",
                "Choose an expense category.",
                "OK");

            return;
        }


        // Build the Expense object
        Expense expense =
            new Expense(
                0,
                expenseName,
                amount,
                category,
                ExpenseDatePicker.Date
                    ?? DateTime.Today);


        // Save to SQLite
        dataBaseManager.AddExpense(
            expense);


        // Close modal
        CloseModals();


        // Refresh list and monthly total
        LoadTransactions();
    }



    // ==========================================
    // TRANSACTION CLICKED
    // ==========================================

    private void TransactionSelected(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
        {
            return;
        }


        TransactionDisplayItem? selectedItem =
            e.CurrentSelection[0]
                as TransactionDisplayItem;


        if (selectedItem == null)
        {
            return;
        }


        // Store real Expense object
        selectedExpense =
            selectedItem.Expense;


        // Fill edit form
        EditExpenseNameEntry.Text =
            selectedExpense.Name;


        EditExpenseAmountEntry.Text =
            selectedExpense.Amount
                .ToString("0.00");


        EditExpenseDatePicker.Date =
            selectedExpense.Date;


        // --------------------------------------
        // FIND EXISTING CATEGORY
        // --------------------------------------

        List<string>? categories =
            EditExpenseCategoryPicker
                .ItemsSource
                as List<string>;


        if (categories != null)
        {
            EditExpenseCategoryPicker
                .SelectedIndex =
                categories.FindIndex(
                    category =>
                        category.Equals(
                            selectedExpense.Category,
                            StringComparison.OrdinalIgnoreCase));
        }


        // Make sure Add modal is closed
        AddExpenseModal.IsVisible =
            false;


        // Show Edit modal
        ModalBackground.IsVisible =
            true;

        EditExpenseModal.IsVisible =
            true;


        // Remove selection highlight
        TransactionsCollectionView
            .SelectedItem = null;
    }



    // ==========================================
    // CANCEL EDIT
    // ==========================================

    private void CancelEditExpenseClicked(
        object? sender,
        EventArgs e)
    {
        CloseModals();
    }



    // ==========================================
    // SAVE CHANGES
    // ==========================================

    private async void SaveExpenseChangesClicked(
        object? sender,
        EventArgs e)
    {
        if (selectedExpense == null)
        {
            return;
        }


        string name =
            EditExpenseNameEntry.Text?
                .Trim() ?? "";


        string amountText =
            EditExpenseAmountEntry.Text?
                .Trim() ?? "";


        string category =
            EditExpenseCategoryPicker
                .SelectedItem?
                .ToString() ?? "";


        // Validate name
        if (string.IsNullOrWhiteSpace(
                name))
        {
            await DisplayAlertAsync(
                "Missing Name",
                "Enter a name for the expense.",
                "OK");

            return;
        }


        // Validate amount
        if (!double.TryParse(
                amountText,
                out double amount)
            ||
            amount <= 0)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Enter a valid expense amount.",
                "OK");

            return;
        }


        // Validate category
        if (string.IsNullOrWhiteSpace(
                category))
        {
            await DisplayAlertAsync(
                "Missing Category",
                "Choose an expense category.",
                "OK");

            return;
        }


        // Keep the original database Id
        Expense updatedExpense =
            new Expense(
                selectedExpense.Id,
                name,
                amount,
                category,
                EditExpenseDatePicker.Date
                    ?? DateTime.Today);


        // Update SQLite
        dataBaseManager.UpdateExpense(
            updatedExpense);


        // Close modal
        CloseModals();


        // Refresh page
        LoadTransactions();
    }



    // ==========================================
    // DELETE EXPENSE
    // ==========================================

    private async void DeleteExpenseClicked(
        object? sender,
        EventArgs e)
    {
        if (selectedExpense == null)
        {
            return;
        }


        bool deleteConfirmed =
            await DisplayAlertAsync(
                "Delete Transaction",
                $"Are you sure you want to delete {selectedExpense.Name}?",
                "Delete",
                "Cancel");


        if (!deleteConfirmed)
        {
            return;
        }


        // Delete from SQLite
        dataBaseManager.DeleteExpenseById(
            selectedExpense.Id);


        // Close modal
        CloseModals();


        // Refresh page
        LoadTransactions();
    }



    // ==========================================
    // TRANSACTION DISPLAY MODEL
    // ==========================================

    public class TransactionDisplayItem
    {
        public Expense Expense { get; }


        public string Name =>
            Expense.Name;


        public string Category =>
            Expense.Category;


        public string DateText =>
            Expense.Date.ToString("MMM d");


        public string AmountText =>
            Expense.Amount.ToString("C");


        public TransactionDisplayItem(
            Expense expense)
        {
            Expense = expense;
        }
    }
}