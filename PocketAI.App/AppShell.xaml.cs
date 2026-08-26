namespace PocketAI.App;

public partial class AppShell : Shell
{
    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public AppShell()
    {
        InitializeComponent();


        // Home is selected when the app
        // first opens.
        SetActiveNav(
            HomeNav,
            HomeIndicator);
    }



    // ==========================================
    // SET ACTIVE NAVIGATION ITEM
    // ==========================================

    private void SetActiveNav(
        Border activeNav,
        BoxView activeIndicator)
    {
        // ======================================
        // ALL NAVIGATION ITEMS
        // ======================================

        Border[] navItems =
        {
            HomeNav,
            TransactionsNav,
            BudgetNav,
            SavingsNav,
            BillsNav,
            AccountsNav,
            AnalyticsNav,
            PocketAINav,
            SettingsNav,
            ProfileNav
        };



        // ======================================
        // ALL ACTIVE INDICATORS
        // ======================================

        BoxView[] indicators =
        {
            HomeIndicator,
            TransactionsIndicator,
            BudgetIndicator,
            SavingsIndicator,
            BillsIndicator,
            AccountsIndicator,
            AnalyticsIndicator,
            PocketAIIndicator,
            SettingsIndicator,
            ProfileIndicator
        };



        // ======================================
        // CLEAR NAVIGATION BACKGROUNDS
        // ======================================

        foreach (Border nav
                 in navItems)
        {
            nav.BackgroundColor =
                Colors.Transparent;
        }



        // ======================================
        // CLEAR ACTIVE INDICATORS
        // ======================================

        foreach (BoxView indicator
                 in indicators)
        {
            indicator.BackgroundColor =
                Colors.Transparent;
        }



        // ======================================
        // SELECTED NAVIGATION BACKGROUND
        // ======================================

        if (Application.Current?.Resources[
                "SidebarSelectedBackground"]
            is Color selectedBackground)
        {
            activeNav.BackgroundColor =
                selectedBackground;
        }



        // ======================================
        // SELECTED THEME INDICATOR
        // ======================================

        if (Application.Current?.Resources[
                "ThemePrimary"]
            is Color themePrimary)
        {
            activeIndicator.BackgroundColor =
                themePrimary;
        }
    }



    // ==========================================
    // HOME
    // ==========================================

    private async void HomeClicked(
        object? sender,
        EventArgs e)
    {
        SetActiveNav(
            HomeNav,
            HomeIndicator);


        await GoToAsync(
            "//Home");
    }



    // ==========================================
    // TRANSACTIONS
    // ==========================================

    private async void TransactionsClicked(
        object? sender,
        EventArgs e)
    {
        SetActiveNav(
            TransactionsNav,
            TransactionsIndicator);


        await GoToAsync(
            "//Transactions");
    }



    // ==========================================
    // BUDGET
    // ==========================================

    private async void BudgetClicked(
        object? sender,
        EventArgs e)
    {
        SetActiveNav(
            BudgetNav,
            BudgetIndicator);


        await GoToAsync(
            "//Budget");
    }



    // ==========================================
    // SAVINGS
    // ==========================================

    private async void SavingsClicked(
        object? sender,
        EventArgs e)
    {
        SetActiveNav(
            SavingsNav,
            SavingsIndicator);


        await GoToAsync(
            "//Savings");
    }



    // ==========================================
    // BILLS
    // ==========================================

    private async void BillsClicked(
        object? sender,
        EventArgs e)
    {
        SetActiveNav(
            BillsNav,
            BillsIndicator);


        await GoToAsync(
            "//Bills");
    }



    // ==========================================
    // ACCOUNTS
    // ==========================================

    private async void AccountsClicked(
        object? sender,
        EventArgs e)
    {
        SetActiveNav(
            AccountsNav,
            AccountsIndicator);


        await GoToAsync(
            "//Accounts");
    }



    // ==========================================
    // ANALYTICS
    // ==========================================

    private async void AnalyticsClicked(
        object? sender,
        EventArgs e)
    {
        SetActiveNav(
            AnalyticsNav,
            AnalyticsIndicator);


        await GoToAsync(
            "//Analytics");
    }



    // ==========================================
    // POCKETAI
    // ==========================================

    private async void PocketAIClicked(
        object? sender,
        EventArgs e)
    {
        SetActiveNav(
            PocketAINav,
            PocketAIIndicator);


        await GoToAsync(
            "//PocketAI");
    }



    // ==========================================
    // SETTINGS
    // ==========================================

    private async void SettingsClicked(
        object? sender,
        EventArgs e)
    {
        SetActiveNav(
            SettingsNav,
            SettingsIndicator);


        await GoToAsync(
            "//Settings");
    }



    // ==========================================
    // PROFILE
    // ==========================================

    private async void ProfileClicked(
        object? sender,
        EventArgs e)
    {
        SetActiveNav(
            ProfileNav,
            ProfileIndicator);


        await GoToAsync(
            "//Profile");
    }
}