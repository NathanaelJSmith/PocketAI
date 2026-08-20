namespace PocketAI.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

		SetActiveNav(HomeNav, HomeIndicator);
    }


	private void SetActiveNav(
    Border activeNav,
    BoxView activeIndicator)
    {
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

        // Clears every navigation item
        foreach (Border nav in navItems)
        {
            nav.BackgroundColor = Colors.Transparent;
        }

        // Clears every selection indicator
        foreach (BoxView indicator in indicators)
        {
            indicator.BackgroundColor = Colors.Transparent;
        }

        activeNav.BackgroundColor =
            Color.FromArgb("#2A2340");

        activeIndicator.BackgroundColor =
            Color.FromArgb("#A78BFA");
    }

    private async void HomeClicked(object? sender, EventArgs e)
    {
        SetActiveNav(HomeNav, HomeIndicator);
        
        await GoToAsync("//Home");
    }

    private async void TransactionsClicked(object? sender, EventArgs e)
    {
        SetActiveNav(TransactionsNav, TransactionsIndicator);
        await GoToAsync("//Transactions");
    }

    private async void BudgetClicked(object? sender, EventArgs e)
    {
        SetActiveNav(BudgetNav, BudgetIndicator);
        await GoToAsync("//Budget");
    }

    private async void SavingsClicked(object? sender, EventArgs e)
    {
        SetActiveNav(SavingsNav, SavingsIndicator);
        await GoToAsync("//Savings");
    }

    private async void BillsClicked(object? sender, EventArgs e)
    {
        SetActiveNav(BillsNav, BillsIndicator);
        await GoToAsync("//Bills");
    }

    private async void AccountsClicked(object? sender, EventArgs e)
    {
        SetActiveNav (AccountsNav, AccountsIndicator);
        await GoToAsync("//Accounts");
    }

    private async void AnalyticsClicked(object? sender, EventArgs e)
    {
        SetActiveNav(AnalyticsNav, AnalyticsIndicator);
        await GoToAsync("//Analytics");
    }

    private async void PocketAIClicked(object? sender, EventArgs e)
    {
        SetActiveNav(PocketAINav, PocketAIIndicator);
        await GoToAsync("//PocketAI");
    }

    private async void SettingsClicked(object? sender, EventArgs e)
    {
        SetActiveNav(SettingsNav, SettingsIndicator);
        await GoToAsync("//Settings");
    }

    private async void ProfileClicked(object? sender, EventArgs e)
    {
        SetActiveNav(ProfileNav, ProfileIndicator);
        await GoToAsync("//Profile");
    }

}