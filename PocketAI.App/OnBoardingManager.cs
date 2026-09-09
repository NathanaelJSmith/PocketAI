public static class OnBoardingManager
{
    private const string OnboardingCompletekey = "OnboardingComplete";

    public static bool IsComplete => Preferences.Default.Get(OnboardingCompletekey, false);

    public static void MarkComplete()
    {
        Preferences.Default.Set(OnboardingCompletekey, true);
    }

    public static void Reset()
    {
        Preferences.Default.Remove(OnboardingCompletekey);
    }
}