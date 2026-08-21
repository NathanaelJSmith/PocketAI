namespace PocketAI.App.Pages;

public partial class BillsPage : ContentPage
{
    private readonly DataBaseManager dataBaseManager;
    private readonly AnalyticsService analyticsService;

    // All recurring bills from SQLite
    private List<RecurringExpenses> bills =
        new List<RecurringExpenses>();

    // Bill currently being edited
    private RecurringExpenses? selectedBill;


    public BillsPage()
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

        LoadBills();
    }



    // ==========================================
    // BILL CATEGORIES
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
    // LOAD BILLS
    // ==========================================

    private void LoadBills()
    {
        bills =
            dataBaseManager
                .GetRecuringExpenses();


        List<BillDisplayItem> displayItems =
            bills
                .OrderByDescending(
                    bill => bill.IsActive)
                .ThenBy(
                    bill => bill.IsActive
                        ? analyticsService
                            .GetDaysUntilDue(
                                bill.DueDay)
                        : int.MaxValue)
                .Select(
                    bill =>
                        new BillDisplayItem(
                            bill,
                            analyticsService))
                .ToList();


        BindableLayout.SetItemsSource(
            BillsContainer,
            displayItems);


        BillsEmptyState.IsVisible =
            bills.Count == 0;


        UpdateSummary();
    }



    // ==========================================
    // UPDATE SUMMARY CARDS
    // ==========================================

    private void UpdateSummary()
    {
        List<RecurringExpenses> activeBills =
            bills
                .Where(
                    bill => bill.IsActive)
                .ToList();


        double monthlyTotal =
            activeBills.Sum(
                bill => bill.Amount);


        int inactiveCount =
            bills.Count(
                bill => !bill.IsActive);


        MonthlyBillsLabel.Text =
            monthlyTotal.ToString("C");


        ActiveBillsLabel.Text =
            activeBills.Count.ToString();


        InactiveBillsLabel.Text =
            inactiveCount.ToString();


        RecurringExpenses? nextBill =
            activeBills
                .OrderBy(
                    bill =>
                        analyticsService
                            .GetDaysUntilDue(
                                bill.DueDay))
                .FirstOrDefault();


        if (nextBill == null)
        {
            NextBillLabel.Text =
                "None";
        }
        else
        {
            int days =
                analyticsService
                    .GetDaysUntilDue(
                        nextBill.DueDay);


            if (days == 0)
            {
                NextBillLabel.Text =
                    $"{nextBill.Name} • Today";
            }
            else if (days == 1)
            {
                NextBillLabel.Text =
                    $"{nextBill.Name} • Tomorrow";
            }
            else
            {
                NextBillLabel.Text =
                    $"{nextBill.Name} • {days} days";
            }
        }
    }



    // ==========================================
    // SHOW ADD BILL
    // ==========================================

    private void ShowAddBillClicked(
        object? sender,
        EventArgs e)
    {
        selectedBill =
            null;


        BillModalTitleLabel.Text =
            "ADD BILL";


        SaveBillButton.Text =
            "Add Bill";


        DeleteBillButton.IsVisible =
            false;


        BillNameEntry.Text =
            "";


        BillCategoryPicker.SelectedIndex =
            -1;


        BillAmountEntry.Text =
            "";


        BillDueDayEntry.Text =
            "";


        BillActiveSwitch.IsToggled =
            true;


        ModalBackground.IsVisible =
            true;


        BillModal.IsVisible =
            true;
    }



    // ==========================================
    // SHOW EDIT BILL
    // ==========================================

    private void ShowEditBillClicked(
        object? sender,
        EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }


        if (button.BindingContext
            is not BillDisplayItem item)
        {
            return;
        }


        selectedBill =
            item.Bill;


        BillModalTitleLabel.Text =
            "EDIT BILL";


        SaveBillButton.Text =
            "Save Changes";


        DeleteBillButton.IsVisible =
            true;


        BillNameEntry.Text =
            selectedBill.Name;


        BillCategoryPicker.SelectedItem =
            selectedBill.Category;


        BillAmountEntry.Text =
            selectedBill.Amount
                .ToString("0.00");


        BillDueDayEntry.Text =
            selectedBill.DueDay
                .ToString();


        BillActiveSwitch.IsToggled =
            selectedBill.IsActive;


        ModalBackground.IsVisible =
            true;


        BillModal.IsVisible =
            true;
    }



    // ==========================================
    // SAVE BILL
    // ==========================================

    private async void SaveBillClicked(
        object? sender,
        EventArgs e)
    {
        string name =
            BillNameEntry.Text?
                .Trim() ?? "";


        string category =
            BillCategoryPicker
                .SelectedItem?
                .ToString() ?? "";


        string amountText =
            BillAmountEntry.Text?
                .Trim() ?? "";


        string dueDayText =
            BillDueDayEntry.Text?
                .Trim() ?? "";


        // --------------------------------------
        // NAME
        // --------------------------------------

        if (string.IsNullOrWhiteSpace(
                name))
        {
            await DisplayAlertAsync(
                "Missing Name",
                "Enter a name for the bill.",
                "OK");

            return;
        }


        // --------------------------------------
        // CATEGORY
        // --------------------------------------

        if (string.IsNullOrWhiteSpace(
                category))
        {
            await DisplayAlertAsync(
                "Missing Category",
                "Select a category.",
                "OK");

            return;
        }


        // --------------------------------------
        // AMOUNT
        // --------------------------------------

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


        // --------------------------------------
        // DUE DAY
        // --------------------------------------

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


        bool isActive =
            BillActiveSwitch.IsToggled;


        // ======================================
        // ADD NEW BILL
        // ======================================

        if (selectedBill == null)
        {
            RecurringExpenses newBill =
                new RecurringExpenses(
                    0,
                    name,
                    category,
                    amount,
                    dueDay,
                    isActive);


            dataBaseManager
                .AddRecurringExpense(
                    newBill);
        }

        // ======================================
        // UPDATE EXISTING BILL
        // ======================================

        else
        {
            RecurringExpenses updatedBill =
                new RecurringExpenses(
                    selectedBill.Id,
                    name,
                    category,
                    amount,
                    dueDay,
                    isActive);


            dataBaseManager
                .UpdateRecurringExpense(
                    updatedBill);
        }


        CloseBillModal();

        LoadBills();
    }



    // ==========================================
    // ACTIVE / INACTIVE QUICK TOGGLE
    // ==========================================

    private void ToggleBillActiveClicked(
        object? sender,
        EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }


        if (button.BindingContext
            is not BillDisplayItem item)
        {
            return;
        }


        RecurringExpenses bill =
            item.Bill;


        RecurringExpenses updatedBill =
            new RecurringExpenses(
                bill.Id,
                bill.Name,
                bill.Category,
                bill.Amount,
                bill.DueDay,
                !bill.IsActive);


        dataBaseManager
            .UpdateRecurringExpense(
                updatedBill);


        LoadBills();
    }



    // ==========================================
    // DELETE BILL
    // ==========================================

    private async void DeleteBillClicked(
        object? sender,
        EventArgs e)
    {
        if (selectedBill == null)
        {
            return;
        }


        bool confirmed =
            await DisplayAlertAsync(
                "Delete Bill",
                $"Delete {selectedBill.Name}?",
                "Delete",
                "Cancel");


        if (!confirmed)
        {
            return;
        }


        dataBaseManager
            .DeleteRecurringExpenseById(
                selectedBill.Id);


        CloseBillModal();

        LoadBills();
    }



    // ==========================================
    // CANCEL
    // ==========================================

    private void CancelBillModalClicked(
        object? sender,
        EventArgs e)
    {
        CloseBillModal();
    }



    // ==========================================
    // CLICK DARK BACKGROUND
    // ==========================================

    private void CloseBillModalBackgroundClicked(
        object? sender,
        TappedEventArgs e)
    {
        CloseBillModal();
    }



    // ==========================================
    // CLOSE MODAL
    // ==========================================

    private void CloseBillModal()
    {
        BillModal.IsVisible =
            false;


        ModalBackground.IsVisible =
            false;


        selectedBill =
            null;
    }



    // ==========================================
    // DISPLAY MODEL
    // ==========================================

    public class BillDisplayItem
    {
        private readonly AnalyticsService
            analyticsService;


        public RecurringExpenses Bill { get; }


        public string Name =>
            Bill.Name;


        public string Category =>
            Bill.Category;


        public string AmountText =>
            Bill.Amount.ToString("C");


        public string ActiveStatusText =>
            Bill.IsActive
                ? "ACTIVE"
                : "INACTIVE";


        public string ToggleText =>
            Bill.IsActive
                ? "Pause"
                : "Activate";


        public Color StatusColor =>
            Bill.IsActive
                ? Color.FromArgb("#15803D")
                : Color.FromArgb("#6B7280");


        public Color StatusBackgroundColor =>
            Bill.IsActive
                ? Color.FromArgb("#DCFCE7")
                : Color.FromArgb("#F3F4F6");


        public string DueDateText =>
            $"Day {Bill.DueDay}";


        public int DaysUntilDue
        {
            get
            {
                if (!Bill.IsActive)
                {
                    return 0;
                }


                return analyticsService
                    .GetDaysUntilDue(
                        Bill.DueDay);
            }
        }


        public string NextPaymentText
        {
            get
            {
                if (!Bill.IsActive)
                {
                    return "Paused";
                }


                DateTime today =
                    DateTime.Today;


                int validDayThisMonth =
                    Math.Min(
                        Bill.DueDay,
                        DateTime.DaysInMonth(
                            today.Year,
                            today.Month));


                DateTime dueDate =
                    new DateTime(
                        today.Year,
                        today.Month,
                        validDayThisMonth);


                if (dueDate.Date <
                    today.Date)
                {
                    DateTime nextMonth =
                        today.AddMonths(1);


                    int validDayNextMonth =
                        Math.Min(
                            Bill.DueDay,
                            DateTime.DaysInMonth(
                                nextMonth.Year,
                                nextMonth.Month));


                    dueDate =
                        new DateTime(
                            nextMonth.Year,
                            nextMonth.Month,
                            validDayNextMonth);
                }


                return dueDate
                    .ToString("MMM d");
            }
        }


        public string DueStatusText
        {
            get
            {
                if (!Bill.IsActive)
                {
                    return "Paused";
                }


                if (DaysUntilDue == 0)
                {
                    return "Due today";
                }


                if (DaysUntilDue == 1)
                {
                    return "Due tomorrow";
                }


                return
                    $"Due in {DaysUntilDue} days";
            }
        }


        public Color DueStatusColor
        {
            get
            {
                if (!Bill.IsActive)
                {
                    return Color.FromArgb(
                        "#6B7280");
                }


                if (DaysUntilDue <= 2)
                {
                    return Color.FromArgb(
                        "#B91C1C");
                }


                if (DaysUntilDue <= 7)
                {
                    return Color.FromArgb(
                        "#B45309");
                }


                return Color.FromArgb(
                    "#15803D");
            }
        }


        public BillDisplayItem(
            RecurringExpenses bill,
            AnalyticsService analyticsService)
        {
            Bill =
                bill;


            this.analyticsService =
                analyticsService;
        }
    }
}