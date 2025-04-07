using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;

namespace RunPlan.ViewModel;

public class RouteDrawable : IDrawable
{

    public ObservableCollection<Location> RouteCoordinates { get; set; } = new();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {

        Debug.WriteLine($"Route has {RouteCoordinates.Count} points.");

        if (RouteCoordinates.Count < 2)
            return; // Not enough points to draw a route

        double minLat = RouteCoordinates.Min(p => p.Latitude);
        double maxLat = RouteCoordinates.Max(p => p.Latitude);
        double minLon = RouteCoordinates.Min(p => p.Longitude);
        double maxLon = RouteCoordinates.Max(p => p.Longitude);

        double latRange = maxLat - minLat;
        double lonRange = maxLon - minLon;
        if (latRange == 0 || lonRange == 0)
            return;

        float MapX(double lon) => (float)((lon - minLon) / lonRange * dirtyRect.Width);
        float MapY(double lat) => (float)((1 - (lat - minLat) / latRange) * dirtyRect.Height);

        var path = new PathF();
        bool isFirstPoint = true;

        foreach (var location in RouteCoordinates)
        {
            float x = MapX(location.Longitude);
            float y = MapY(location.Latitude);

            if (isFirstPoint)
            {
                path.MoveTo(x, y);
                isFirstPoint = false;
            }
            else
            {
                path.LineTo(x, y);
            }
        }

        canvas.StrokeColor = Colors.Blue;
        canvas.StrokeSize = 5;
        canvas.DrawPath(path);
    }
}