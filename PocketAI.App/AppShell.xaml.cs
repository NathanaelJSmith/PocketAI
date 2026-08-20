namespace PocketAI.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

		SetActiveButton(HomeButton);
    }

	private void SetActiveButton(Button activeButton)
	{
    	Button[] buttons =
    	{
        HomeButton,
        TransactionsButton,
        BudgetButton,
        SavingsButton,
        BillsButton,
        AccountsButton,
        AnalyticsButton,
        PocketAIButton,
        SettingsButton,
        ProfileButton
    	};

    foreach (Button button in buttons)
    	{
        button.BackgroundColor = Colors.Transparent;
        button.TextColor = Color.FromArgb("#E5E7EB");
    	}

    	activeButton.BackgroundColor = Color.FromArgb("#A78BFA");
    	activeButton.TextColor = Color.FromArgb("#111827");
	}

    private async void HomeClicked(object? sender, EventArgs e)
    {
        SetActiveButton(HomeButton);
        
        await GoToAsync("//Home");
    }

    private async void TransactionsClicked(object? sender, EventArgs e)
    {
        SetActiveButton(TransactionsButton);
        await GoToAsync("//Transactions");
    }

    private async void BudgetClicked(object? sender, EventArgs e)
    {
        SetActiveButton(BudgetButton);
        await GoToAsync("//Budget");
    }

    private async void SavingsClicked(object? sender, EventArgs e)
    {
        SetActiveButton(SavingsButton);
        await GoToAsync("//Savings");
    }

    private async void BillsClicked(object? sender, EventArgs e)
    {
        SetActiveButton(BillsButton);
        await GoToAsync("//Bills");
    }

    private async void AccountsClicked(object? sender, EventArgs e)
    {
        SetActiveButton (AccountsButton);
        await GoToAsync("//Accounts");
    }

    private async void AnalyticsClicked(object? sender, EventArgs e)
    {
        SetActiveButton(AnalyticsButton);
        await GoToAsync("//Analytics");
    }

    private async void PocketAIClicked(object? sender, EventArgs e)
    {
        SetActiveButton(PocketAIButton);
        await GoToAsync("//PocketAI");
    }

    private async void SettingsClicked(object? sender, EventArgs e)
    {
        SetActiveButton(SettingsButton);
        await GoToAsync("//Settings");
    }

    private async void ProfileClicked(object? sender, EventArgs e)
    {
        SetActiveButton(ProfileButton);
        await GoToAsync("//Profile");
    }

}