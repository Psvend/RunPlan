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

    private double _totalDistance;
    public double TotalDistance
    {
        get => _totalDistance;
        set => SetProperty(ref _totalDistance, value);
    }

    public ICommand StartTrackingCommand { get; }
    public ICommand StopTrackingCommand { get; }
    public event Action? RouteUpdated;  // Notify when route updates


    private bool _isTracking;


    public MappingViewModel(IGeolocation geolocation)
    {
        _geolocation = geolocation;
        RouteDrawable.RouteCoordinates = RouteCoordinates;

        StartTrackingCommand = new RelayCommand(async () => await StartTrackingAsync());
        StopTrackingCommand = new RelayCommand(StopTracking);
    }

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

                // Calculate distance
                if (previousLocation != null)
                {
                    double distance = Location.CalculateDistance(previousLocation, location, DistanceUnits.Kilometers);
                    TotalDistance += distance;
                    Debug.WriteLine($"Distance added: {distance} km, Total Distance: {TotalDistance} km");
                }

                previousLocation = location;

                // Refresh the UI
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RouteDrawable.RouteCoordinates = RouteCoordinates;
                    RouteUpdated?.Invoke();  // Event to refresh GraphicsView
                });
            }

            await Task.Delay(5000); // Adjust tracking interval
        }
    }

    private void StopTracking()
    {
        _isTracking = false;
        Debug.WriteLine($"Tracking stopped. Total Distance: {TotalDistance} km");
    }
}