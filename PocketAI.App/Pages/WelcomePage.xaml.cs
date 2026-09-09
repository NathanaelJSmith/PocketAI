namespace PocketAI.App.Pages;

public partial class WelcomePage : ContentPage
{
    public WelcomePage()
    {
        InitializeComponent();
    }

    private async void GetStartedClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new IncomeSetupPage());
    }
}