namespace PocketAI.App.Pages;


public partial class TransactionsPage : ContentPage
{
    private readonly DataBaseManager
        dataBaseManager;


    // Stores every expense loaded from SQLite.
    private List<Expense> allExpenses =
        new List<Expense>();


    // Stores the expense currently being edited.
    private Expense? selectedExpense;


    // Prevents accidental duplicate submissions.
    private bool isSavingExpense =
        false;



    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public TransactionsPage()
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


        SetupMonths();
    }



    // ==========================================
    // PAGE APPEARS
    // ==========================================

    protected override void OnAppearing()
    {
        base.OnAppearing();


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


        // Main transaction filter.
        CategoryPicker.ItemsSource =
            categories;


        // Add Expense picker.
        ExpenseCategoryPicker.ItemsSource =
            categories
                .Skip(1)
                .ToList();


        // Edit Expense picker.
        EditExpenseCategoryPicker.ItemsSource =
            categories
                .Skip(1)
                .ToList();


        CategoryPicker.SelectedIndex =
            0;
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


        // Add the most recent 12 months.
        for (int i = 0;
             i < 12;
             i++)
        {
            months.Add(
                currentMonth
                    .AddMonths(
                        -i)
                    .ToString(
                        "MMMM yyyy"));
        }


        MonthPicker.ItemsSource =
            months;


        // Default to current month.
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
            thisMonthTotal
                .ToString(
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
        IEnumerable<Expense>
            filteredExpenses =
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
                filteredExpenses
                    .Where(
                        expense =>
                            expense.Name.Contains(
                                searchText,
                                StringComparison
                                    .OrdinalIgnoreCase)
                            ||
                            expense.Category.Contains(
                                searchText,
                                StringComparison
                                    .OrdinalIgnoreCase));
        }



        // --------------------------------------
        // CATEGORY FILTER
        // --------------------------------------

        if (CategoryPicker.SelectedIndex >
            0)
        {
            string selectedCategory =
                CategoryPicker
                    .SelectedItem?
                    .ToString()
                ??
                "";


            filteredExpenses =
                filteredExpenses
                    .Where(
                        expense =>
                            expense.Category.Equals(
                                selectedCategory,
                                StringComparison
                                    .OrdinalIgnoreCase));
        }



        // --------------------------------------
        // MONTH FILTER
        // --------------------------------------

        if (MonthPicker.SelectedIndex >
            0)
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
                    filteredExpenses
                        .Where(
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
            displayItems.Count ==
            0;


        TransactionsCollectionView.IsVisible =
            displayItems.Count >
            0;
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


        ExpenseDatePicker.Date =
            DateTime.Today;



        // Make sure Edit modal is closed.
        EditExpenseModal.IsVisible =
            false;


        // Show Add Expense modal.
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
        if (isSavingExpense)
        {
            return;
        }


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


        // ======================================
        // CHECKING IS THE ONLY SPENDING ACCOUNT
        // ======================================
        //
        // Cash has been removed from PocketAI.
        //
        // Transactions automatically reduce
        // Checking.
        // ======================================

        const string paidFromAccount =
            "Checking";



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
        // VALIDATE USEFUL NAME
        // ======================================

        if (!expenseName.Any(
                character =>
                    char.IsLetter(
                        character)))
        {
            await DisplayAlertAsync(
                "Invalid Name",
                "The expense name must contain at least one letter.",
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
            !double.IsFinite(
                amount)
            ||
            amount <= 0)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Enter a valid expense amount greater than zero.",
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
        // VALIDATE DATE
        // ======================================

        DateTime expenseDate =
            ExpenseDatePicker.Date
            ??
            DateTime.Today;


        if (expenseDate.Date >
            DateTime.Today)
        {
            await DisplayAlertAsync(
                "Invalid Date",
                "A transaction cannot be dated in the future.",
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
                expenseDate,
                paidFromAccount);



        // ======================================
        // SAVE TO SQLITE
        // ======================================
        //
        // DataBaseManager will:
        //
        // 1. Save the expense.
        // 2. Reduce Checking by the amount.
        // 3. Commit both changes together.
        // ======================================

        try
        {
            isSavingExpense =
                true;


            dataBaseManager
                .AddExpense(
                    expense);


            CloseModals();


            LoadTransactions();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Failed to add expense: {ex}");


            await DisplayAlertAsync(
                "Unable to Save",
                "PocketAI could not save this transaction. Your data was not changed. Please try again.",
                "OK");
        }
        finally
        {
            isSavingExpense =
                false;
        }
    }



    // ==========================================
    // TRANSACTION CLICKED
    // ==========================================

    private void TransactionSelected(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count ==
            0)
        {
            return;
        }


        TransactionDisplayItem?
            selectedItem =
                e.CurrentSelection[0]
                    as TransactionDisplayItem;


        if (selectedItem ==
            null)
        {
            return;
        }


        selectedExpense =
            selectedItem.Expense;



        // ======================================
        // FILL EDIT FORM
        // ======================================

        EditExpenseNameEntry.Text =
            selectedExpense.Name;


        EditExpenseAmountEntry.Text =
            selectedExpense
                .Amount
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


        if (categories !=
            null)
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



        // Make sure Add modal is closed.
        AddExpenseModal.IsVisible =
            false;


        // Show Edit modal.
        ModalBackground.IsVisible =
            true;


        EditExpenseModal.IsVisible =
            true;


        // Remove CollectionView highlight.
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
        if (selectedExpense ==
            null)
        {
            return;
        }


        if (isSavingExpense)
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


        // Every transaction belongs to Checking.
        const string paidFromAccount =
            "Checking";



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
        // VALIDATE USEFUL NAME
        // ======================================

        if (!name.Any(
                character =>
                    char.IsLetter(
                        character)))
        {
            await DisplayAlertAsync(
                "Invalid Name",
                "The expense name must contain at least one letter.",
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
            !double.IsFinite(
                amount)
            ||
            amount <= 0)
        {
            await DisplayAlertAsync(
                "Invalid Amount",
                "Enter a valid expense amount greater than zero.",
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
        // VALIDATE DATE
        // ======================================

        DateTime expenseDate =
            EditExpenseDatePicker.Date
            ??
            DateTime.Today;


        if (expenseDate.Date >
            DateTime.Today)
        {
            await DisplayAlertAsync(
                "Invalid Date",
                "A transaction cannot be dated in the future.",
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
                expenseDate,
                paidFromAccount);



        // ======================================
        // UPDATE SQLITE
        // ======================================
        //
        // DataBaseManager will:
        //
        // 1. Restore the original transaction
        //    amount to Checking.
        //
        // 2. Update the transaction.
        //
        // 3. Subtract the updated amount from
        //    Checking.
        //
        // Example:
        //
        // Original transaction:
        // $20
        //
        // Updated transaction:
        // $30
        //
        // Checking is restored by $20,
        // then reduced by $30.
        //
        // Net change = -$10.
        // ======================================

        try
        {
            isSavingExpense =
                true;


            dataBaseManager
                .UpdateExpense(
                    updatedExpense);


            CloseModals();


            LoadTransactions();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Failed to update expense: {ex}");


            await DisplayAlertAsync(
                "Unable to Save",
                "PocketAI could not update this transaction. Your original transaction was not changed. Please try again.",
                "OK");
        }
        finally
        {
            isSavingExpense =
                false;
        }
    }



    // ==========================================
    // DELETE EXPENSE
    // ==========================================

    private async void DeleteExpenseClicked(
        object? sender,
        EventArgs e)
    {
        if (selectedExpense ==
            null)
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
        //
        // DataBaseManager restores the money to
        // Checking when the transaction is
        // deleted.
        // ======================================

        try
        {
            dataBaseManager
                .DeleteExpenseById(
                    selectedExpense.Id);


            CloseModals();


            LoadTransactions();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Failed to delete expense: {ex}");


            await DisplayAlertAsync(
                "Unable to Delete",
                "PocketAI could not delete this transaction. Your transaction and checking balance were not changed. Please try again.",
                "OK");
        }
    }



    // ==========================================
    // TRANSACTION DISPLAY MODEL
    // ==========================================

    public class TransactionDisplayItem
    {
        public Expense Expense
        {
            get;
        }


        public string Name =>
            Expense.Name;


        public string Category =>
            Expense.Category;


        public string DateText =>
            Expense.Date.ToString(
                "MMM d");


        public string AmountText =>
            Expense.Amount.ToString(
                "C");



        // ======================================
        // CONSTRUCTOR
        // ======================================

        public TransactionDisplayItem(
            Expense expense)
        {
            Expense =
                expense;
        }
    }
}