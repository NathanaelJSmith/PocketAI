namespace PocketAI.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();


        // Load the user's saved appearance.
        ThemeManager.ApplySavedTheme();


        // Listen for Windows / Android / iOS
        // system theme changes.
        RequestedThemeChanged +=
            OnRequestedThemeChanged;
    }


    // ==========================================
    // SYSTEM THEME CHANGED
    // ==========================================

    private void OnRequestedThemeChanged(
        object? sender,
        AppThemeChangedEventArgs e)
    {
        ThemeManager.ApplySystemTheme(
            e.RequestedTheme);
    }


    protected override Window CreateWindow(
        IActivationState? activationState)
    {
        return new Window(
            new AppShell());
    }
}