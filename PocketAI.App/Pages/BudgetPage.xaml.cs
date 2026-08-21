namespace PocketAI.App.Pages;

public partial class BudgetPage : ContentPage
{
    private readonly DataBaseManager dataBaseManager;
    private readonly AnalyticsService analyticsService;

    // All saved budget limits
    private List<BudgetLimit> allBudgets =
        new List<BudgetLimit>();

    // Budget currently being edited
    private BudgetLimit? selectedBudget;


    public BudgetPage()
    {
        InitializeComponent();

        string databasePath =
            Path.Combine(
                FileSystem.AppDataDirectory,
                "pocketai.db");

        dataBaseManager =
            new DataBaseManager(databasePath);

        analyticsService =
            new AnalyticsService();

        dataBaseManager.CreateTables();

        SetupCategories();
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadBudgets();
    }



    // ==========================================
    // CATEGORY OPTIONS
    // ==========================================

    private void SetupCategories()
    {
        List<string> categories =
            new List<string>
            {
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

        BudgetCategoryPicker.ItemsSource =
            categories;
    }



    // ==========================================
    // LOAD BUDGET DATA
    // ==========================================

    private void LoadBudgets()
    {
        allBudgets =
            dataBaseManager
                .GetBudgetLimits()
                .OrderBy(budget =>
                    budget.Category)
                .ToList();


        List<Expense> allExpenses =
            dataBaseManager
                .GetAllExpenses();


        List<Expense> currentMonthExpenses =
            analyticsService
                .GetCurrentMonthExpense(
                    allExpenses);


        List<BudgetDisplayItem> displayItems =
            new List<BudgetDisplayItem>();


        // Summary calculations
        double totalBudget = 0;
        double totalSpent = 0;
        int overBudgetCount = 0;


        foreach (BudgetLimit budget in allBudgets)
        {
            double spent =
                analyticsService
                    .GetCategoryTotal(
                        currentMonthExpenses,
                        budget.Category);


            BudgetDisplayItem item =
                new BudgetDisplayItem(
                    budget,
                    spent);


            displayItems.Add(item);


            totalBudget +=
                budget.LimitAmount;

            totalSpent +=
                spent;


            if (spent > budget.LimitAmount)
            {
                overBudgetCount++;
            }
        }


        double remaining =
            totalBudget - totalSpent;


        // Update summary cards
        TotalBudgetLabel.Text =
            totalBudget.ToString("C");

        BudgetSpentLabel.Text =
            totalSpent.ToString("C");

        BudgetRemainingLabel.Text =
            remaining.ToString("C");

        OverBudgetCountLabel.Text =
            overBudgetCount.ToString();


        // Remaining turns red if below zero
        if (remaining < 0)
        {
            BudgetRemainingLabel.TextColor =
                Color.FromArgb("#B91C1C");
        }
        else
        {
            BudgetRemainingLabel.TextColor =
                Color.FromArgb("#111827");
        }


        BudgetCollectionView.ItemsSource =
            displayItems;


        bool hasBudgets =
            displayItems.Count > 0;


        BudgetEmptyState.IsVisible =
            !hasBudgets;

        BudgetCollectionView.IsVisible =
            hasBudgets;
    }



    // ==========================================
    // SHOW ADD BUDGET
    // ==========================================

    private void ShowAddBudgetClicked(
        object? sender,
        EventArgs e)
    {
        selectedBudget = null;


        BudgetModalTitleLabel.Text =
            "ADD BUDGET";

        SaveBudgetButton.Text =
            "Add Budget";


        DeleteBudgetButton.IsVisible =
            false;


        BudgetCategoryPicker.SelectedIndex =
            -1;

        BudgetLimitEntry.Text =
            "";


        ModalBackground.IsVisible =
            true;

        BudgetModal.IsVisible =
            true;
    }



    // ==========================================
    // SELECT EXISTING BUDGET
    // ==========================================

    private void BudgetSelected(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
        {
            return;
        }


        BudgetDisplayItem? selectedItem =
            e.CurrentSelection[0]
                as BudgetDisplayItem;


        if (selectedItem == null)
        {
            return;
        }


        selectedBudget =
            selectedItem.Budget;


        BudgetModalTitleLabel.Text =
            "EDIT BUDGET";

        SaveBudgetButton.Text =
            "Save Changes";

        DeleteBudgetButton.IsVisible =
            true;


        BudgetLimitEntry.Text =
            selectedBudget
                .LimitAmount
                .ToString("0.00");


        // Find existing category in picker
        List<string>? categories =
            BudgetCategoryPicker.ItemsSource
                as List<string>;


        if (categories != null)
        {
            BudgetCategoryPicker.SelectedIndex =
                categories.FindIndex(
                    category =>
                        category.Equals(
                            selectedBudget.Category,
                            StringComparison.OrdinalIgnoreCase));
        }


        ModalBackground.IsVisible =
            true;

        BudgetModal.IsVisible =
            true;


        BudgetCollectionView.SelectedItem =
            null;
    }



    // ==========================================
    // SAVE ADD / EDIT
    // ==========================================

    private async void SaveBudgetClicked(
        object? sender,
        EventArgs e)
    {
        string category =
            BudgetCategoryPicker
                .SelectedItem?
                .ToString() ?? "";


        string amountText =
            BudgetLimitEntry.Text?
                .Trim() ?? "";


        // Validate category
        if (string.IsNullOrWhiteSpace(
                category))
        {
            await DisplayAlertAsync(
                "Missing Category",
                "Choose a budget category.",
                "OK");

            return;
        }


        // Validate limit
        if (!double.TryParse(
                amountText,
                out double limitAmount)
            ||
            limitAmount <= 0)
        {
            await DisplayAlertAsync(
                "Invalid Limit",
                "Enter a valid monthly budget amount.",
                "OK");

            return;
        }


        // ======================================
        // ADDING NEW BUDGET
        // ======================================

        if (selectedBudget == null)
        {
            bool categoryAlreadyExists =
                allBudgets.Any(
                    budget =>
                        budget.Category.Equals(
                            category,
                            StringComparison.OrdinalIgnoreCase));


            if (categoryAlreadyExists)
            {
                await DisplayAlertAsync(
                    "Budget Already Exists",
                    $"You already have a budget for {category}. Click that budget to edit it.",
                    "OK");

                return;
            }


            BudgetLimit newBudget =
                new BudgetLimit(
                    category,
                    limitAmount);


            dataBaseManager.SaveBudgetLimit(
                newBudget);
        }

        // ======================================
        // EDITING EXISTING BUDGET
        // ======================================

        else
        {
            bool categoryUsedByAnotherBudget =
                allBudgets.Any(
                    budget =>
                        !ReferenceEquals(
                            budget,
                            selectedBudget)
                        &&
                        budget.Category.Equals(
                            category,
                            StringComparison.OrdinalIgnoreCase));


            if (categoryUsedByAnotherBudget)
            {
                await DisplayAlertAsync(
                    "Budget Already Exists",
                    $"You already have another budget for {category}.",
                    "OK");

                return;
            }


            // Your current database layer doesn't
            // have UpdateBudgetLimit yet.
            //
            // Delete the old one, then save the
            // edited version.

            dataBaseManager
                .DeleteBudgetLimitsByCategory(
                    selectedBudget.Category);


            BudgetLimit updatedBudget =
                new BudgetLimit(
                    category,
                    limitAmount);


            dataBaseManager.SaveBudgetLimit(
                updatedBudget);
        }


        CloseBudgetModal();

        LoadBudgets();
    }



    // ==========================================
    // DELETE BUDGET
    // ==========================================

    private async void DeleteBudgetClicked(
        object? sender,
        EventArgs e)
    {
        if (selectedBudget == null)
        {
            return;
        }


        bool deleteConfirmed =
            await DisplayAlertAsync(
                "Delete Budget",
                $"Delete the {selectedBudget.Category} budget?",
                "Delete",
                "Cancel");


        if (!deleteConfirmed)
        {
            return;
        }


        dataBaseManager
            .DeleteBudgetLimitsByCategory(
                selectedBudget.Category);


        CloseBudgetModal();

        LoadBudgets();
    }



    // ==========================================
    // CANCEL
    // ==========================================

    private void CancelBudgetClicked(
        object? sender,
        EventArgs e)
    {
        CloseBudgetModal();
    }



    // ==========================================
    // CLICK DARK BACKGROUND
    // ==========================================

    private void CloseBudgetModalClicked(
        object? sender,
        TappedEventArgs e)
    {
        CloseBudgetModal();
    }



    // ==========================================
    // CLOSE MODAL
    // ==========================================

    private void CloseBudgetModal()
    {
        BudgetModal.IsVisible =
            false;

        ModalBackground.IsVisible =
            false;

        selectedBudget =
            null;
    }



    // ==========================================
    // DISPLAY MODEL
    // ==========================================

    public class BudgetDisplayItem
    {
        public BudgetLimit Budget { get; }

        public string Category =>
            Budget.Category;


        public double Spent { get; }


        public double Remaining =>
            Budget.LimitAmount - Spent;


        public bool IsOverBudget =>
            Remaining < 0;


        public double PercentUsed
        {
            get
            {
                if (Budget.LimitAmount <= 0)
                {
                    return 0;
                }

                return
                    (Spent / Budget.LimitAmount)
                    * 100;
            }
        }


        public double Progress =>
            Math.Clamp(
                PercentUsed / 100.0,
                0,
                1);


        public string SpentOfLimitText =>
            $"{Spent:C} of {Budget.LimitAmount:C}";


        public string SpentText =>
            $"{Spent:C} spent";


        public string PercentText =>
            $"{PercentUsed:F0}% used";


        public string RemainingText =>
            IsOverBudget
                ? $"{Math.Abs(Remaining):C} over"
                : $"{Remaining:C} left";


        public string StatusText
        {
            get
            {
                if (IsOverBudget)
                {
                    return "Over budget";
                }

                if (PercentUsed >= 80)
                {
                    return "Getting close";
                }

                return "On track";
            }
        }


        public Color ProgressColor
        {
            get
            {
                if (IsOverBudget)
                {
                    return Color.FromArgb(
                        "#DC2626");
                }

                if (PercentUsed >= 80)
                {
                    return Color.FromArgb(
                        "#D97706");
                }

                return Color.FromArgb(
                    "#7C3AED");
            }
        }


        public Color RemainingColor =>
            IsOverBudget
                ? Color.FromArgb("#B91C1C")
                : Color.FromArgb("#15803D");


        public Color StatusColor =>
            ProgressColor;


        public BudgetDisplayItem(
            BudgetLimit budget,
            double spent)
        {
            Budget =
                budget;

            Spent =
                spent;
        }
    }
}