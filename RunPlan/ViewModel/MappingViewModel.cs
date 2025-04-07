using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;


namespace RunPlan.ViewModel;

public partial class MappingViewModel : BaseVievModel
{
    private readonly IGeolocation _geolocation;
    public ObservableCollection<Location> RouteCoordinates { get; } = new();
    public RouteDrawable RouteDrawable { get; } = new();

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
    private void StopTracking()
    {
        _isTracking = false;
        Debug.WriteLine($"Tracking stopped. Total Distance: {TotalDistance} km");
    }
}