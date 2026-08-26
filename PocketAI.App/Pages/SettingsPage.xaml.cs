using PocketAI.App;

namespace PocketAI.App.Pages;

public partial class SettingsPage : ContentPage
{
    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public SettingsPage()
    {
        InitializeComponent();


        // Display the saved theme name.
        UpdateCurrentThemeLabel();
        UpdateDisplayModeLabel();
    }



    // ==========================================
    // PAGE APPEARS
    // ==========================================

    protected override void OnAppearing()
    {
        base.OnAppearing();


        UpdateCurrentThemeLabel();
        UpdateDisplayModeLabel();
    }



    // ==========================================
    // PURPLE
    // ==========================================

    private void PurpleThemeClicked(
        object? sender,
        EventArgs e)
    {
        ChangeTheme(
            "Purple");
    }



    // ==========================================
    // BLUE
    // ==========================================

    private void BlueThemeClicked(
        object? sender,
        EventArgs e)
    {
        ChangeTheme(
            "Blue");
    }



    // ==========================================
    // GREEN
    // ==========================================

    private void GreenThemeClicked(
        object? sender,
        EventArgs e)
    {
        ChangeTheme(
            "Green");
    }



    // ==========================================
    // TEAL
    // ==========================================

    private void TealThemeClicked(
        object? sender,
        EventArgs e)
    {
        ChangeTheme(
            "Teal");
    }



    // ==========================================
    // ORANGE
    // ==========================================

    private void OrangeThemeClicked(
        object? sender,
        EventArgs e)
    {
        ChangeTheme(
            "Orange");
    }



    // ==========================================
    // PINK
    // ==========================================

    private void PinkThemeClicked(
        object? sender,
        EventArgs e)
    {
        ChangeTheme(
            "Pink");
    }



    // ==========================================
    // RED
    // ==========================================

    private void RedThemeClicked(
        object? sender,
        EventArgs e)
    {
        ChangeTheme(
            "Red");
    }



    // ==========================================
    // CHANGE THEME
    // ==========================================

    private void ChangeTheme(
        string themeName)
    {
        ThemeManager.ApplyTheme(
            themeName);


        CurrentThemeLabel.Text =
            themeName;
    }



    // ==========================================
    // UPDATE CURRENT THEME LABEL
    // ==========================================

    private void UpdateCurrentThemeLabel()
    {
        CurrentThemeLabel.Text =
            ThemeManager.GetCurrentTheme();
    }

    // ==========================================
    // LIGHT MODE
    // ==========================================

    private void LightModeClicked(
        object? sender,
        EventArgs e)
    {
        ThemeManager.ApplyDisplayMode(
            "Light");


        UpdateDisplayModeLabel();
    }



    // ==========================================
    // DARK MODE
    // ==========================================

    private void DarkModeClicked(
        object? sender,
        EventArgs e)
    {
        ThemeManager.ApplyDisplayMode(
            "Dark");


        UpdateDisplayModeLabel();
    }



    // ==========================================
    // SYSTEM MODE
    // ==========================================

    private void SystemModeClicked(
        object? sender,
        EventArgs e)
    {
        ThemeManager.ApplyDisplayMode(
            "System");


        UpdateDisplayModeLabel();
    }



    // ==========================================
    // UPDATE DISPLAY MODE LABEL
    // ==========================================

    private void UpdateDisplayModeLabel()
    {
        string displayMode =
            ThemeManager.GetDisplayMode();


        CurrentDisplayModeLabel.Text =
            $"Current mode: {displayMode}";
    }
}