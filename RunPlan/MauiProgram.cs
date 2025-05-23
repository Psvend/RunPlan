﻿using Microsoft.Extensions.Logging;
using RunPlan.Data;

namespace RunPlan;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register SQLite DatabaseService
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        Task.Run(async () => await InitializeDatabaseAsync(app));

        return app;
    }

    private static async Task InitializeDatabaseAsync(MauiApp app)
    {
        var dbService = app.Services.GetRequiredService<DatabaseService>();

        // Insert a test activity
        await dbService.InsertRunningActivity("Evening Run", 7.5, "00:40:00","2025-02-24");

        // Fetch and print activities
        var activities = await dbService.GetAllActivitiesAsync();
        foreach (var activity in activities)
        {
            Console.WriteLine($"📌 {activity.Name}, {activity.Distance} km, {activity.Time}, {activity.Date}");
        }
    }
}
