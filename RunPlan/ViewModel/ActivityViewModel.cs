using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunPlan.Data;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RunPlan.Messages;




namespace RunPlan.ViewModel;

public partial class ActivityViewModel : BaseVievModel
{
    public ObservableCollection<RunningActivity> RunningActivities { get; } = new();

    private readonly DatabaseService _databaseService;

    private bool _isTracking;
    private Location? _previousLocation;
    private DateTime _startTime;
    private Timer? _timer;

    private readonly IGeolocation _geolocation;

    public ObservableCollection<String> diffeculties { get; } = new()
    {
        "Easy",
        "Moderate",
        "Hard"
    };





    public ActivityViewModel(DatabaseService databaseService, IGeolocation geolocation)
    {
        _geolocation = geolocation;
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
    [ObservableProperty] private string grade;
    [ObservableProperty] private string description;

    [ObservableProperty] private bool startButtonVisible = true;
    [ObservableProperty] private bool stopButtonVisible = false;


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
    [ObservableProperty]
    private string selectedDiffeculty;


    [RelayCommand]
    public async Task AddActivity()
    {
        if (string.IsNullOrWhiteSpace(ActivityName) ||
            string.IsNullOrWhiteSpace(DistanceText) ||
            string.IsNullOrWhiteSpace(Time) ||
            string.IsNullOrWhiteSpace(Date) ||
            string.IsNullOrWhiteSpace(Grade) ||
            string.IsNullOrWhiteSpace(Description))
        {
            await Shell.Current.DisplayAlert("Error", "Please fill in all fields.", "OK");
            return;
        }

        if (!double.TryParse(DistanceText, out double distance))
        {
            await Shell.Current.DisplayAlert("Error", "Distance must be a valid number.", "OK");
            return;
        }

        await _databaseService.InsertRunningActivity(ActivityName, distance, Time, Date, Grade, Description);

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

        await Shell.Current.GoToAsync(nameof(ActivityDetail), true, new Dictionary<string, object>
        {
            { "RunningActivity", activity }
        });
    }
    [RelayCommand]
    private async Task StartTrackingAsync()
    {
        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        _isTracking = true;
        StartButtonVisible = false;
        StopButtonVisible = true;
        _startTime = DateTime.Now;
        _previousLocation = null;
        DistanceText = "0";
        Date = _startTime.ToString("yyyy-MM-dd");

        _timer = new Timer(async _ => await UpdateLocationAsync(), null, 0, 5000);
    }

    [RelayCommand]
    private void StopTracking()
    {
        _isTracking = false;
        StartButtonVisible = true;
        StopButtonVisible = false;
        _timer?.Dispose();
        Time = (DateTime.Now - _startTime).ToString(@"hh\:mm\:ss");
    }

    private async Task UpdateLocationAsync()
    {
        if (!_isTracking) return;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                var location = await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));

                if (location != null && _previousLocation != null)
                {
                    var distance = Location.CalculateDistance(_previousLocation, location, DistanceUnits.Kilometers);
                    if (double.TryParse(DistanceText, out double current))
                        DistanceText = (current + distance).ToString("F2");
                }

                _previousLocation = location;
            }
            catch (Exception ex)
            {
                // Optional: log or show error to user
                Debug.WriteLine($"Location error: {ex.Message}");
            }
        });
    }




}
