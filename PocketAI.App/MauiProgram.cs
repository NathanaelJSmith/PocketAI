using LiveChartsCore.SkiaSharpView.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace PocketAI.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder =
            MauiApp.CreateBuilder();


        builder
            // Required by LiveCharts2
            .UseSkiaSharp()

            // Registers LiveCharts2 with MAUI
            .UseLiveCharts()

            .UseMauiApp<App>()

            .ConfigureFonts(fonts =>
            {
                fonts.AddFont(
                    "OpenSans-Regular.ttf",
                    "OpenSansRegular");

                fonts.AddFont(
                    "OpenSans-Semibold.ttf",
                    "OpenSansSemibold");
            });


        return builder.Build();
    }
}
