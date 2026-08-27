namespace PocketAI.App.Pages;

public partial class TransactionsPage : ContentPage
{
    private readonly DataBaseManager dataBaseManager;

    // Stores every expense loaded from SQLite
    private List<Expense> allExpenses =
        new List<Expense>();

    // Stores the expense currently being edited
    private Expense? selectedExpense;


    public TransactionsPage()
    {
        InitializeComponent();

        // Use the same MAUI database as the other pages
        string databasePath =
            Path.Combine(
                FileSystem.AppDataDirectory,
                "pocketai.db");


        dataBaseManager =
            new DataBaseManager(
                databasePath);


        dataBaseManager.CreateTables();


        SetupCategories();
        SetupMonths();
        SetupPaymentAccounts();
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
        List<string> categories =
            new List<string>
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
            categories
                .Skip(1)
                .ToList();


        // Edit Expense picker
        EditExpenseCategoryPicker.ItemsSource =
            categories
                .Skip(1)
                .ToList();


        CategoryPicker.SelectedIndex =
            0;
    }



    // ==========================================
    // PAYMENT ACCOUNTS
    // ==========================================

    private void SetupPaymentAccounts()
    {
        List<string> paymentAccounts =
            new List<string>
            {
                "Checking",
                "Cash"
            };


        // Add Expense account picker
        ExpensePaidFromPicker.ItemsSource =
            paymentAccounts;


        // Edit Expense account picker
        EditExpensePaidFromPicker.ItemsSource =
            paymentAccounts;
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
                    .ToString(
                        "MMMM yyyy"));
        }


        MonthPicker.ItemsSource =
            months;


        // Default to current month
        MonthPicker.SelectedIndex =
            1;
    }



    // ==========================================
    // LOAD TRANSACTIONS
    // ==========================================

    private void LoadTransactions()
    {
        allExpenses =
            dataBaseManager
                .GetAllExpenses()
                .OrderByDescending(
                    expense =>
                        expense.Date)
                .ThenByDescending(
                    expense =>
                        expense.Id)
                .ToList();


        ApplyFilters();


        // ======================================
        // THIS MONTH TOTAL
        // ======================================

        DateTime today =
            DateTime.Today;


        double thisMonthTotal =
            allExpenses
                .Where(
                    expense =>
                        expense.Date.Year ==
                            today.Year
                        &&
                        expense.Date.Month ==
                            today.Month)
                .Sum(
                    expense =>
                        expense.Amount);


        ThisMonthTotalLabel.Text =
            thisMonthTotal.ToString(
                "C");
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
            TransactionSearchBar.Text?
                .Trim()
            ??
            "";


        if (!string.IsNullOrWhiteSpace(
                searchText))
        {
            filteredExpenses =
                filteredExpenses.Where(
                    expense =>
                        expense.Name.Contains(
                            searchText,
                            StringComparison
                                .OrdinalIgnoreCase)
                        ||
                        expense.Category.Contains(
                            searchText,
                            StringComparison
                                .OrdinalIgnoreCase)
                        ||
                        (
                            expense.PaidFromAccount
                                != null
                            &&
                            expense
                                .PaidFromAccount
                                .Contains(
                                    searchText,
                                    StringComparison
                                        .OrdinalIgnoreCase)
                        ));
        }


        // --------------------------------------
        // CATEGORY FILTER
        // --------------------------------------

        if (CategoryPicker.SelectedIndex > 0)
        {
            string selectedCategory =
                CategoryPicker
                    .SelectedItem?
                    .ToString()
                ??
                "";


            filteredExpenses =
                filteredExpenses.Where(
                    expense =>
                        expense.Category.Equals(
                            selectedCategory,
                            StringComparison
                                .OrdinalIgnoreCase));
        }


        // --------------------------------------
        // MONTH FILTER
        // --------------------------------------

        if (MonthPicker.SelectedIndex > 0)
        {
            string selectedMonth =
                MonthPicker
                    .SelectedItem?
                    .ToString()
                ??
                "";


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


        // ======================================
        // BUILD DISPLAY ITEMS
        // ======================================

        List<TransactionDisplayItem>
            displayItems =
                filteredExpenses
                    .Select(
                        expense =>
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
        selectedExpense =
            null;


        // ======================================
        // CLEAR OLD VALUES
        // ======================================

        ExpenseNameEntry.Text =
            "";


        ExpenseAmountEntry.Text =
            "";


        ExpenseCategoryPicker.SelectedIndex =
            -1;


        // Checking is the default payment account
        ExpensePaidFromPicker.SelectedIndex =
            0;


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
            ExpenseNameEntry.Text?
                .Trim()
            ??
            "";


        string amountText =
            ExpenseAmountEntry.Text?
                .Trim()
            ??
            "";


        string category =
            ExpenseCategoryPicker
                .SelectedItem?
                .ToString()
            ??
            "";


        string paidFromAccount =
            ExpensePaidFromPicker
                .SelectedItem?
                .ToString()
            ??
            "";


        // ======================================
        // VALIDATE NAME
        // ======================================

        if (string.IsNullOrWhiteSpace(
                expenseName))
        {
            await DisplayAlertAsync(
                "Missing Name",
                "Enter a name for the expense.",
                "OK");


            return;
        }


        // ======================================
        // VALIDATE AMOUNT
        // ======================================

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


        // ======================================
        // VALIDATE CATEGORY
        // ======================================

        if (string.IsNullOrWhiteSpace(
                category))
        {
            await DisplayAlertAsync(
                "Missing Category",
                "Choose an expense category.",
                "OK");


            return;
        }


        // ======================================
        // VALIDATE PAYMENT ACCOUNT
        // ======================================

        if (string.IsNullOrWhiteSpace(
                paidFromAccount))
        {
            await DisplayAlertAsync(
                "Missing Account",
                "Choose which account paid for this expense.",
                "OK");


            return;
        }


        // ======================================
        // BUILD EXPENSE
        // ======================================

        Expense expense =
            new Expense(
                0,
                expenseName,
                amount,
                category,
                ExpenseDatePicker.Date
                    ?? DateTime.Today,
                paidFromAccount);


        // ======================================
        // SAVE TO SQLITE
        // ======================================

        // DataBaseManager will:
        //
        // 1. Save the expense
        // 2. Reduce Checking or Cash
        // 3. Commit both changes together

        dataBaseManager.AddExpense(
            expense);


        // Close modal
        CloseModals();


        // Refresh transaction list
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


        // ======================================
        // FILL EDIT FORM
        // ======================================

        EditExpenseNameEntry.Text =
            selectedExpense.Name;


        EditExpenseAmountEntry.Text =
            selectedExpense.Amount
                .ToString(
                    "0.00");


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
                            StringComparison
                                .OrdinalIgnoreCase));
        }


        // --------------------------------------
        // FIND EXISTING PAYMENT ACCOUNT
        // --------------------------------------

        List<string>? paymentAccounts =
            EditExpensePaidFromPicker
                .ItemsSource
                as List<string>;


        if (paymentAccounts != null)
        {
            EditExpensePaidFromPicker
                .SelectedIndex =
                paymentAccounts.FindIndex(
                    account =>
                        account.Equals(
                            selectedExpense
                                .PaidFromAccount,
                            StringComparison
                                .OrdinalIgnoreCase));
        }


        // Old transactions may not have a
        // PaidFromAccount yet.
        //
        // In that situation FindIndex returns -1,
        // which leaves the picker unselected.


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
            .SelectedItem =
            null;
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
                .Trim()
            ??
            "";


        string amountText =
            EditExpenseAmountEntry.Text?
                .Trim()
            ??
            "";


        string category =
            EditExpenseCategoryPicker
                .SelectedItem?
                .ToString()
            ??
            "";


        string paidFromAccount =
            EditExpensePaidFromPicker
                .SelectedItem?
                .ToString()
            ??
            "";


        // ======================================
        // VALIDATE NAME
        // ======================================

        if (string.IsNullOrWhiteSpace(
                name))
        {
            await DisplayAlertAsync(
                "Missing Name",
                "Enter a name for the expense.",
                "OK");


            return;
        }


        // ======================================
        // VALIDATE AMOUNT
        // ======================================

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


        // ======================================
        // VALIDATE CATEGORY
        // ======================================

        if (string.IsNullOrWhiteSpace(
                category))
        {
            await DisplayAlertAsync(
                "Missing Category",
                "Choose an expense category.",
                "OK");


            return;
        }


        // ======================================
        // VALIDATE PAYMENT ACCOUNT
        // ======================================

        if (string.IsNullOrWhiteSpace(
                paidFromAccount))
        {
            await DisplayAlertAsync(
                "Missing Account",
                "Choose which account paid for this expense.",
                "OK");


            return;
        }


        // ======================================
        // BUILD UPDATED EXPENSE
        // ======================================

        Expense updatedExpense =
            new Expense(
                selectedExpense.Id,
                name,
                amount,
                category,
                EditExpenseDatePicker.Date
                    ?? DateTime.Today,
                paidFromAccount);


        // ======================================
        // UPDATE SQLITE
        // ======================================

        // DataBaseManager will:
        //
        // 1. Restore the old account effect
        // 2. Update the transaction
        // 3. Apply the new account effect
        //
        // Example:
        //
        // Old:
        // $20 from Checking
        //
        // New:
        // $30 from Cash
        //
        // Checking +$20
        // Cash     -$30

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


        // ======================================
        // DELETE FROM SQLITE
        // ======================================

        // DataBaseManager will restore the money
        // to the original account before deleting
        // the transaction.

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
            Expense.Date.ToString(
                "MMM d");


        public string PaidFromText =>
            string.IsNullOrWhiteSpace(
                Expense.PaidFromAccount)

                ? "Not set"

                : Expense.PaidFromAccount;


        public string AmountText =>
            Expense.Amount.ToString(
                "C");


        public TransactionDisplayItem(
            Expense expense)
        {
            Expense =
                expense;
        }
    }
}