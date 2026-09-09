namespace PocketAI.App.Pages;


public partial class MeetPocketAIPage : ContentPage
{
    public MeetPocketAIPage()
    {
        InitializeComponent();
    }


    // ==========================================
    // ENTER POCKETAI
    // ==========================================
    //
    // Final onboarding completion and startup
    // routing will be connected after this page
    // is confirmed working.
    // ==========================================

    private void EnterPocketAIClicked(
    object? sender,
    EventArgs e)
    {
        OnBoardingManager.MarkComplete();


        if (Application.Current == null
            ||
            Application.Current.Windows.Count == 0)
        {
            return;
        }


        Application.Current
            .Windows[0]
            .Page =
                new AppShell();
    }
}