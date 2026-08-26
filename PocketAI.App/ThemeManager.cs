using Microsoft.Maui.Storage;

namespace PocketAI.App;

public static class ThemeManager
{
    // ==========================================
    // SAVED SETTINGS
    // ==========================================

    private const string ThemePreferenceKey =
        "BudgetAIAccentTheme";

    private const string DisplayModePreferenceKey =
        "BudgetAIDisplayMode";


    // ==========================================
    // LOAD SAVED APPEARANCE
    // ==========================================

    public static void ApplySavedTheme()
    {
        string savedDisplayMode =
            GetDisplayMode();

        ApplyDisplayMode(
            savedDisplayMode,
            false);
    }


    // ==========================================
    // ACCENT THEME
    // ==========================================

    public static void ApplyTheme(
        string themeName,
        bool savePreference = true)
    {
        string primary;
        string primaryDark;

        string lightPrimaryLight;
        string lightPrimaryVeryLight;

        string darkPrimaryLight;
        string darkPrimaryVeryLight;


        switch (themeName)
        {
            // ==================================
            // BLUE
            // ==================================

            case "Blue":

                primary =
                    "#2563EB";

                primaryDark =
                    "#1D4ED8";

                lightPrimaryLight =
                    "#DBEAFE";

                lightPrimaryVeryLight =
                    "#EFF6FF";

                darkPrimaryLight =
                    "#1E3A5F";

                darkPrimaryVeryLight =
                    "#172554";

                break;


            // ==================================
            // GREEN
            // ==================================

            case "Green":

                primary =
                    "#16A34A";

                primaryDark =
                    "#15803D";

                lightPrimaryLight =
                    "#DCFCE7";

                lightPrimaryVeryLight =
                    "#F0FDF4";

                darkPrimaryLight =
                    "#14532D";

                darkPrimaryVeryLight =
                    "#052E16";

                break;


            // ==================================
            // TEAL
            // ==================================

            case "Teal":

                primary =
                    "#0D9488";

                primaryDark =
                    "#0F766E";

                lightPrimaryLight =
                    "#CCFBF1";

                lightPrimaryVeryLight =
                    "#F0FDFA";

                darkPrimaryLight =
                    "#134E4A";

                darkPrimaryVeryLight =
                    "#042F2E";

                break;


            // ==================================
            // ORANGE
            // ==================================

            case "Orange":

                primary =
                    "#EA580C";

                primaryDark =
                    "#C2410C";

                lightPrimaryLight =
                    "#FFEDD5";

                lightPrimaryVeryLight =
                    "#FFF7ED";

                darkPrimaryLight =
                    "#7C2D12";

                darkPrimaryVeryLight =
                    "#431407";

                break;


            // ==================================
            // PINK
            // ==================================

            case "Pink":

                primary =
                    "#DB2777";

                primaryDark =
                    "#BE185D";

                lightPrimaryLight =
                    "#FCE7F3";

                lightPrimaryVeryLight =
                    "#FDF2F8";

                darkPrimaryLight =
                    "#831843";

                darkPrimaryVeryLight =
                    "#500724";

                break;


            // ==================================
            // RED
            // ==================================

            case "Red":

                primary =
                    "#DC2626";

                primaryDark =
                    "#B91C1C";

                lightPrimaryLight =
                    "#FEE2E2";

                lightPrimaryVeryLight =
                    "#FEF2F2";

                darkPrimaryLight =
                    "#7F1D1D";

                darkPrimaryVeryLight =
                    "#450A0A";

                break;


            // ==================================
            // PURPLE
            // ==================================

            default:

                themeName =
                    "Purple";

                primary =
                    "#7C3AED";

                primaryDark =
                    "#6D28D9";

                lightPrimaryLight =
                    "#EDE9FE";

                lightPrimaryVeryLight =
                    "#F5F3FF";

                darkPrimaryLight =
                    "#4C3575";

                darkPrimaryVeryLight =
                    "#25163F";

                break;
        }


        // ======================================
        // LIGHT OR DARK ACCENT SURFACES
        // ======================================

        bool isDark =
            IsDarkModeActive();


        string primaryLight =
            isDark
                ? darkPrimaryLight
                : lightPrimaryLight;


        string primaryVeryLight =
            isDark
                ? darkPrimaryVeryLight
                : lightPrimaryVeryLight;


        // ======================================
        // UPDATE RESOURCES
        // ======================================

        SetColor(
            "ThemePrimary",
            primary);

        SetColor(
            "ThemePrimaryDark",
            primaryDark);

        SetColor(
            "ThemePrimaryLight",
            primaryLight);

        SetColor(
            "ThemePrimaryVeryLight",
            primaryVeryLight);


        // Existing MAUI resources.
        SetColor(
            "Primary",
            primary);

        SetColor(
            "PrimaryDark",
            primaryDark);

        SetColor(
            "Secondary",
            primaryLight);

        SetColor(
            "SecondaryDarkText",
            primary);

        SetColor(
            "Tertiary",
            primaryDark);


        // ======================================
        // SAVE ACCENT
        // ======================================

        if (savePreference)
        {
            Preferences.Default.Set(
                ThemePreferenceKey,
                themeName);
        }
    }


    // ==========================================
    // DISPLAY MODE
    // ==========================================

    public static void ApplyDisplayMode(
        string displayMode,
        bool savePreference = true)
    {
        if (Application.Current == null)
        {
            return;
        }


        bool useDarkColors;


        switch (displayMode)
        {
            // ==================================
            // DARK
            // ==================================

            case "Dark":

                Application.Current.UserAppTheme =
                    AppTheme.Dark;

                useDarkColors =
                    true;

                break;


            // ==================================
            // SYSTEM
            // ==================================

            case "System":

                Application.Current.UserAppTheme =
                    AppTheme.Unspecified;

                useDarkColors =
                    Application.Current.RequestedTheme ==
                    AppTheme.Dark;

                break;


            // ==================================
            // LIGHT
            // ==================================

            default:

                displayMode =
                    "Light";

                Application.Current.UserAppTheme =
                    AppTheme.Light;

                useDarkColors =
                    false;

                break;
        }


        ApplySurfaceColors(
            useDarkColors);


        // Reapply accent because the soft
        // accent colors are different in dark mode.
        ApplyTheme(
            GetCurrentTheme(),
            false);


        if (savePreference)
        {
            Preferences.Default.Set(
                DisplayModePreferenceKey,
                displayMode);
        }
    }


    // ==========================================
    // SYSTEM THEME CHANGED
    // ==========================================

    public static void ApplySystemTheme(
        AppTheme requestedTheme)
    {
        if (GetDisplayMode() != "System")
        {
            return;
        }


        bool useDarkColors =
            requestedTheme ==
            AppTheme.Dark;


        ApplySurfaceColors(
            useDarkColors);


        ApplyTheme(
            GetCurrentTheme(),
            false);
    }


    // ==========================================
    // SURFACE COLORS
    // ==========================================

    private static void ApplySurfaceColors(
        bool darkMode)
    {
        if (darkMode)
        {
            // ==================================
            // DARK MODE
            // ==================================

            SetColor(
                "PageBackground",
                "#0F172A");

            SetColor(
                "CardBackground",
                "#111827");

            SetColor(
                "SurfaceBackground",
                "#1F2937");

            SetColor(
                "BorderColor",
                "#374151");


            SetColor(
                "TextPrimary",
                "#F9FAFB");

            SetColor(
                "TextSecondary",
                "#D1D5DB");

            SetColor(
                "TextMuted",
                "#9CA3AF");

            SetColor(
                "TextOnPrimary",
                "#FFFFFF");


            SetColor(
                "SidebarBackground",
                "#070B14");

            SetColor(
                "SidebarSelectedBackground",
                "#1F2937");

            SetColor(
                "SidebarText",
                "#E5E7EB");

            SetColor(
                "SidebarMutedText",
                "#9CA3AF");
        }
        else
        {
            // ==================================
            // LIGHT MODE
            // ==================================

            SetColor(
                "PageBackground",
                "#F5F7FA");

            SetColor(
                "CardBackground",
                "#FFFFFF");

            SetColor(
                "SurfaceBackground",
                "#F9FAFB");

            SetColor(
                "BorderColor",
                "#E5E7EB");


            SetColor(
                "TextPrimary",
                "#111827");

            SetColor(
                "TextSecondary",
                "#6B7280");

            SetColor(
                "TextMuted",
                "#9CA3AF");

            SetColor(
                "TextOnPrimary",
                "#FFFFFF");


            SetColor(
                "SidebarBackground",
                "#111827");

            SetColor(
                "SidebarSelectedBackground",
                "#1F2937");

            SetColor(
                "SidebarText",
                "#E5E7EB");

            SetColor(
                "SidebarMutedText",
                "#9CA3AF");
        }
    }


    // ==========================================
    // CHECK ACTIVE DISPLAY MODE
    // ==========================================

    private static bool IsDarkModeActive()
    {
        string displayMode =
            GetDisplayMode();


        if (displayMode == "Dark")
        {
            return true;
        }


        if (displayMode == "System" &&
            Application.Current != null)
        {
            return
                Application.Current.RequestedTheme ==
                AppTheme.Dark;
        }


        return false;
    }


    // ==========================================
    // HELPER — SET COLOR RESOURCE
    // ==========================================

    private static void SetColor(
        string resourceName,
        string hexColor)
    {
        if (Application.Current == null)
        {
            return;
        }


        Application.Current.Resources[
            resourceName] =
            Color.FromArgb(
                hexColor);
    }


    // ==========================================
    // GET CURRENT ACCENT
    // ==========================================

    public static string GetCurrentTheme()
    {
        return Preferences.Default.Get(
            ThemePreferenceKey,
            "Purple");
    }


    // ==========================================
    // GET CURRENT DISPLAY MODE
    // ==========================================

    public static string GetDisplayMode()
    {
        return Preferences.Default.Get(
            DisplayModePreferenceKey,
            "Light");
    }
}