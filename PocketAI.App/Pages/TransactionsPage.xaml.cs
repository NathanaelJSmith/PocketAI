namespace PocketAI.App.Pages;

public partial class TransactionsPage : ContentPage
{
    private readonly DataBaseManager dataBaseManager;

    private List<Expense> allExpenses = new List<Expense>();
    private Expense? selectedExpense;


    public TransactionsPage()
    {
        InitializeComponent();

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

        LoadTransactions();
    }


    // ==========================================
    // SETUP FILTERS
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

        CategoryPicker.ItemsSource = categories;

        ExpenseCategoryPicker.ItemsSource =
            categories.Skip(1).ToList();
        
        EditExpenseCategoryPicker.ItemsSource =
            categories.Skip(1).ToList();

        CategoryPicker.SelectedIndex = 0;
    }


    private void SetupMonths()
    {
        List<string> months = new List<string>
        {
            "All Months"
        };

        DateTime currentMonth =
            new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);

        // Gives the user the most recent 12 months
        for (int i = 0; i < 12; i++)
        {
            months.Add(
                currentMonth
                    .AddMonths(-i)
                    .ToString("MMMM yyyy"));
        }

        MonthPicker.ItemsSource = months;

        // Current month
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
                .OrderByDescending(expense => expense.Date)
                .ThenByDescending(expense => expense.Id)
                .ToList();

        ApplyFilters();

        DateTime today = DateTime.Today;

        double thisMonthTotal =
            allExpenses
                .Where(expense =>
                    expense.Date.Year == today.Year &&
                    expense.Date.Month == today.Month)
                .Sum(expense => expense.Amount);

        ThisMonthTotalLabel.Text =
            thisMonthTotal.ToString("C");
    }


    // ==========================================
    // SEARCH + FILTER
    // ==========================================

    private void TransactionSearchChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        ApplyFilters();
    }


    private void FilterChanged(
        object? sender,
        EventArgs e)
    {
        ApplyFilters();
    }


    private void ApplyFilters()
    {
        IEnumerable<Expense> filteredExpenses =
            allExpenses;


        // Search
        string searchText =
            TransactionSearchBar.Text?.Trim() ?? "";

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filteredExpenses =
                filteredExpenses.Where(expense =>
                    expense.Name.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase) ||

                    expense.Category.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase));
        }


        // Category
        if (CategoryPicker.SelectedIndex > 0)
        {
            string selectedCategory =
                CategoryPicker.SelectedItem?.ToString() ?? "";

            filteredExpenses =
                filteredExpenses.Where(expense =>
                    expense.Category.Equals(
                        selectedCategory,
                        StringComparison.OrdinalIgnoreCase));
        }


        // Month
        if (MonthPicker.SelectedIndex > 0)
        {
            string selectedMonth =
                MonthPicker.SelectedItem?.ToString() ?? "";

            if (DateTime.TryParse(
                    $"1 {selectedMonth}",
                    out DateTime monthDate))
            {
                filteredExpenses =
                    filteredExpenses.Where(expense =>
                        expense.Date.Year == monthDate.Year &&
                        expense.Date.Month == monthDate.Month);
            }
        }


        List<TransactionDisplayItem> displayItems =
            filteredExpenses
                .Select(expense =>
                    new TransactionDisplayItem(expense))
                .ToList();


        TransactionsCollectionView.ItemsSource =
            displayItems;

        NoTransactionsLabel.IsVisible =
            displayItems.Count == 0;

        TransactionsCollectionView.IsVisible =
            displayItems.Count > 0;
    }


    // ==========================================
    // SHOW ADD EXPENSE
    // ==========================================

    private void ShowAddExpenseClicked(
        object? sender,
        EventArgs e)
    {
        ExpenseNameEntry.Text = "";
        ExpenseAmountEntry.Text = "";
        ExpenseCategoryPicker.SelectedIndex = -1;
        ExpenseDatePicker.Date = DateTime.Today;

        ModalBackground.IsVisible = true;
        AddExpenseModal.IsVisible = true;
    }


    // ==========================================
    // CANCEL ADD EXPENSE
    // ==========================================

    private void CancelAddExpenseClicked(
        object? sender,
        EventArgs e)
    {
        ModalBackground.IsVisible = false;
        AddExpenseModal.IsVisible = false;

        EditExpenseModal.IsVisible = false;

        selectedExpense = null;
    }


    // ==========================================
    // ADD REAL EXPENSE
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
            ExpenseCategoryPicker.SelectedItem?.ToString() ?? "";


        if (string.IsNullOrWhiteSpace(expenseName))
        {
            await DisplayAlertAsync(
                "Missing Name",
                "Enter a name for the expense.",
                "OK");

            return;
        }


        if (!double.TryParse(
                amountText,
                out double amount) ||
            amount <= 0)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Enter a valid expense amount.",
                "OK");

            return;
        }


        if (string.IsNullOrWhiteSpace(category))
        {
            await DisplayAlertAsync(
                "Missing Category",
                "Choose an expense category.",
                "OK");

            return;
        }


        Expense expense =
            new Expense(
                0,
                expenseName,
                amount,
                category,
                ExpenseDatePicker.Date ?? DateTime.Today);


        dataBaseManager.AddExpense(expense);


        ModalBackground.IsVisible = false;
        AddExpenseModal.IsVisible = false;


        // Reload immediately
        LoadTransactions();
    }


    // ==========================================
    // TRANSACTION SELECTED
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
            e.CurrentSelection[0] as TransactionDisplayItem;

        if (selectedItem == null)
        {
            return;
        }

        // Remember which real Expense was selected
        selectedExpense = selectedItem.Expense;

        // Load its current values into the form
        EditExpenseNameEntry.Text =
            selectedExpense.Name;

        EditExpenseAmountEntry.Text =
            selectedExpense.Amount.ToString("0.00");

        EditExpenseDatePicker.Date =
            selectedExpense.Date;

        // Select the matching category
        List<string>? categories =
            EditExpenseCategoryPicker.ItemsSource
            as List<string>;

        if (categories != null)
        {
            EditExpenseCategoryPicker.SelectedIndex =
                categories.FindIndex(category =>
                    category.Equals(
                        selectedExpense.Category,
                        StringComparison.OrdinalIgnoreCase));
        }

        // Show modal
        ModalBackground.IsVisible = true;
        EditExpenseModal.IsVisible = true;

        // Remove CollectionView highlight
        TransactionsCollectionView.SelectedItem = null;
    }
    // ==========================================
    // CANCEL EDIT
    // ==========================================

    private void CancelEditExpenseClicked(
        object? sender,
        EventArgs e)
    {
        EditExpenseModal.IsVisible = false;
        ModalBackground.IsVisible = false;

        selectedExpense = null;
    }


    // ==========================================
    // SAVE EDITED EXPENSE
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
            EditExpenseNameEntry.Text?.Trim() ?? "";

        string amountText =
            EditExpenseAmountEntry.Text?.Trim() ?? "";

        string category =
            EditExpenseCategoryPicker
                .SelectedItem?.ToString() ?? "";


        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlertAsync(
                "Missing Name",
                "Enter a name for the expense.",
                "OK");

            return;
        }


        if (!double.TryParse(
                amountText,
                out double amount) ||
            amount <= 0)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Enter a valid expense amount.",
                "OK");

            return;
        }


        if (string.IsNullOrWhiteSpace(category))
        {
            await DisplayAlertAsync(
                "Missing Category",
                "Choose an expense category.",
                "OK");

            return;
        }


        // Keep the original database Id,
        // but replace the edited information
        Expense updatedExpense =
            new Expense(
                selectedExpense.Id,
                name,
                amount,
                category,
                EditExpenseDatePicker.Date
                    ?? DateTime.Today);


        dataBaseManager.UpdateExpense(
            updatedExpense);


        EditExpenseModal.IsVisible = false;
        ModalBackground.IsVisible = false;

        selectedExpense = null;


        // Reload immediately
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


        dataBaseManager.DeleteExpenseById(
            selectedExpense.Id);


        EditExpenseModal.IsVisible = false;
        ModalBackground.IsVisible = false;

        selectedExpense = null;


        // Refresh transaction totals and list
        LoadTransactions();
    }


    // ==========================================
    // DISPLAY MODEL
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