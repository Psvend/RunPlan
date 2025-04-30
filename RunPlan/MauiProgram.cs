using Microsoft.Extensions.Logging;
using RunPlan.Data;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Syncfusion.Maui.Core.Hosting;
using RunPlan.ViewModel;
using RunPlan.Model;
using RunPlan.Messages;

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
                fonts.AddFont("OpenSansRegular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
        builder.Services.AddSingleton<IMap>(Map.Default);
        // Register SQLite DatabaseService
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<ActivityList>();
        builder.Services.AddTransient<DetailViewModel>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<ActivityViewModel>();
        builder.Services.AddSingleton<ActivityListViewModel>();
        builder.Services.AddTransient<ActivityDetail>();
        builder.Services.AddSingleton<TrainingListViewModel>();
        builder.Services.AddSingleton<TrainingList>();
        builder.Services.AddTransient<CreateTrainingViewModel>();
        builder.Services.AddTransient<CreateTraining>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<App>();
        builder.Services.AddTransient<TrainingListDetailViewModel>();






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
        //await dbService.InsertRunningActivity("Evening Run", 7.5, "00:40:00","2025-02-24");

        // Fetch and print activities
        var activities = await dbService.GetAllActivitiesAsync();
        foreach (var activity in activities)
        {
            Console.WriteLine($"📌 {activity.Name}, {activity.Distance} km, {activity.Time}, {activity.Date}");
        }
    }
}
