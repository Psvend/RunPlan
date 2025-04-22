using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using System.Timers;


namespace RunPlan.ViewModel;

public partial class MappingViewModel : BaseVievModel
{
    private readonly IGeolocation _geolocation;
    public ObservableCollection<Location> RouteCoordinates { get; } = new();
    public RouteDrawable RouteDrawable { get; } = new();

    private System.Timers.Timer? _timer;
    private TimeSpan _elapsedTime = TimeSpan.Zero;
    private DateTime _startTime;

    public string ElapsedTimeFormatted => _elapsedTime.ToString(@"hh\:mm\:ss");
    public TimeSpan ElapsedTime
    {
        get => _elapsedTime;
        set
        {
            if (SetProperty(ref _elapsedTime, value))
            {
                OnPropertyChanged(nameof(ElapsedTimeFormatted)); // Update formatted string too
            }
        }
    }


    public string FormattedTotalDistance
    {
        get
        {
            if (TotalDistance < 1)
            {
                return $"{TotalDistance * 1000:F0} m"; // Show meters if under 1 km
            }
            else
            {
                return $"{TotalDistance:F2} km"; // Show kilometers
            }
        }
    }


    private double _totalDistance;
    public double TotalDistance
    {
        get => _totalDistance;
        set
        {
            if (SetProperty(ref _totalDistance, value))
            {
                OnPropertyChanged(nameof(FormattedTotalDistance)); // So the label updates too
            }
        }
    }


    public event Action? RouteUpdated;  // Notify when route updates


    private bool _isTracking;


    public MappingViewModel(IGeolocation geolocation)
    {
        _geolocation = geolocation;
        RouteDrawable.RouteCoordinates = RouteCoordinates;

      
    }

    // ✅ Start tracking command (automatically exposed as ICommand)
    [RelayCommand]
    private async Task StartTrackingAsync()
    {
        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            Debug.WriteLine("Location permission denied.");
            return;
        }

        _isTracking = true;
        RouteCoordinates.Clear();
        TotalDistance = 0;
        _startTime = DateTime.Now;
        _timer = new System.Timers.Timer(1000); // 1 second interval
        _timer.Elapsed += (s, e) =>
        {
            ElapsedTime = DateTime.Now - _startTime;
        };
        _timer.Start();

        Location? previousLocation = null;

        while (_isTracking)
        {
            var location = await _geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
            if (location != null)
            {
                RouteCoordinates.Add(location);

                if (previousLocation != null)
                {
                    double distance = Location.CalculateDistance(previousLocation, location, DistanceUnits.Kilometers);
                    TotalDistance += distance;
                    Debug.WriteLine($"Distance added: {distance} km, Total Distance: {TotalDistance} km");
                }

                previousLocation = location;

                MainThread.BeginInvokeOnMainThread(() =>
                {

                    RouteUpdated?.Invoke();  // Notify the UI to refresh GraphicsView
                });
            }

            await Task.Delay(5); // Adjust tracking interval
        }
    }

    // ✅ Stop tracking command
    [RelayCommand]
    private async Task StopTracking()
    {
        _isTracking = false;
        Debug.WriteLine($"Tracking stopped. Total Distance: {TotalDistance} km");
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
        string name = await Application.Current.MainPage.DisplayPromptAsync(
       "Save Run",
       "Give your run a name:",
       "Save",
       "Cancel",
       "Morning Run"
   );

        if (!string.IsNullOrWhiteSpace(name))
        {
            Debug.WriteLine($"Run saved: {name}, Distance: {FormattedTotalDistance}");
            // Optionally add to a saved runs collection here
        }
    }
}