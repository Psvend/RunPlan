using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunPlan.ViewModel;

public class RouteDrawable : IDrawable
{

    public ObservableCollection<Location> RouteCoordinates { get; set; } = new();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (RouteCoordinates.Count < 2)
            return; // Not enough points to draw a route

        // Style for the route line
        canvas.StrokeColor = Colors.Blue;
        canvas.StrokeSize = 5;

        // Scale and translate coordinates for drawing
        var path = new PathF();
        bool isFirstPoint = true;

        foreach (var location in RouteCoordinates)
        {
            var x = (float)(location.Longitude * 10); // Simplified scaling
            var y = (float)(location.Latitude * 10);

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

        // Draw the route
        canvas.DrawPath(path);
    }
}
