using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RunPlan.Data;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RunPlan.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;




namespace RunPlan.ViewModel;

public partial class ActivityListViewModel : BaseVievModel
{
    public ObservableCollection<RunningActivity> RunningActivities { get; } = new();

    private readonly DatabaseService _databaseService;




    public ActivityListViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Running Activities";

        // Initial load
        _ = LoadActivities();
    }

    // 🟣 Input-bound properties
    [ObservableProperty] private string activityName;
    [ObservableProperty] private string distanceText;
    [ObservableProperty] private string time;
    [ObservableProperty] private string date;

    [RelayCommand]
    public async Task LoadActivities()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            RunningActivities.Clear();
            var activities = await _databaseService.GetAllActivitiesAsync();

            foreach (var activity in activities)
                RunningActivities.Add(activity);
        }
        finally
        {
            IsBusy = false;
        }
    }



    [RelayCommand]
    public async Task AddActivity()
    {
        if (string.IsNullOrWhiteSpace(ActivityName) ||
            string.IsNullOrWhiteSpace(DistanceText) ||
            string.IsNullOrWhiteSpace(Time) ||
            string.IsNullOrWhiteSpace(Date))
        {
            await Shell.Current.DisplayAlert("Error", "Please fill in all fields.", "OK");
            return;
        }

        if (!double.TryParse(DistanceText, out double distance))
        {
            await Shell.Current.DisplayAlert("Error", "Distance must be a valid number.", "OK");
            return;
        }

        await _databaseService.InsertRunningActivity(ActivityName, distance, Time, Date);

        // Clear inputs
        ActivityName = DistanceText = Time = Date = string.Empty;

        await LoadActivities();

        // Notify listeners (like MainPage) if necessary
        WeakReferenceMessenger.Default.Send(new ActivityUpdatedMessage());
    }


    [RelayCommand]
    public async Task DeleteActivity(RunningActivity activity)
    {
        if (activity == null) return;

        bool confirm = await Shell.Current.DisplayAlert("Delete", $"Are you sure you want to delete {activity.Name}?", "Yes", "No");
        if (!confirm) return;

        await _databaseService.DeleteActivity(activity.Id);
        await LoadActivities();

        WeakReferenceMessenger.Default.Send(new ActivityUpdatedMessage());
    }



    [RelayCommand]
    public async Task GoToDetails(RunningActivity activity)
    {
        if (activity == null) return;

        await Shell.Current.GoToAsync(nameof(DetailScreen), true, new Dictionary<string, object>
        {
            { "RunningActivity", activity }
        });
    }




}
