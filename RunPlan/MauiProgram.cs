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

        // ✅ Services
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
        builder.Services.AddSingleton<IMap>(Map.Default);
        builder.Services.AddSingleton<DatabaseService>();

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<ActivityList>();
        builder.Services.AddTransient<DetailViewModel>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<ActivityViewModel>();
        builder.Services.AddSingleton<ActivityListViewModel>();
        builder.Services.AddTransient<ActivityDetail>();
        builder.Services.AddSingleton<MappingViewModel>();
        builder.Services.AddSingleton<TrainingListViewModel>();
        builder.Services.AddSingleton<TrainingList>();
        builder.Services.AddTransient<CreateTrainingViewModel>();
        builder.Services.AddTransient<CreateTraining>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // ✅ Build the app
        var app = builder.Build();

        // ✅ Await database initialization before returning app
        InitializeDatabaseBlocking(app);

        return app;
    }

    // This blocks the main thread *once* during setup to avoid Task.Run hacks
    private static void InitializeDatabaseBlocking(MauiApp app)
    {
        var dbService = app.Services.GetRequiredService<DatabaseService>();

        // Run DB initialization synchronously
        dbService.InitializeAsync().GetAwaiter().GetResult();

        // Optionally: log existing activities
        var activities = dbService.GetAllActivitiesAsync().GetAwaiter().GetResult();
        foreach (var activity in activities)
        {
            Console.WriteLine($"📌 {activity.Name}, {activity.Distance} km, {activity.Time}, {activity.Date}");
        }
    }
}
