using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



public class BarChartDrawable : IDrawable
{
    public List<RunningDataModel> Data { get; set; } = new List<RunningDataModel>();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        Console.WriteLine("Drawing Chart...");

        if (Data == null || Data.Count == 0)
        {
            Console.WriteLine("No Data Found!");
            return;
        }

        canvas.FillColor = Colors.Black;
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 2;

        float barWidth = dirtyRect.Width / (Data.Count + 1);
        float maxHeight = dirtyRect.Height - 40;
        float maxValue = (float)Data.Max(d => d.Distance);

        Console.WriteLine($"Max Distance Value: {maxValue}");

        if (maxValue <= 0) maxValue = 1; 

        for (int i = 0; i < Data.Count; i++)
        {
            var item = Data[i];
            Console.WriteLine($"📌 Week: {item.WeekLabel}, Distance: {item.Distance}");

            float barHeight = (float)(item.Distance / maxValue * maxHeight);
            float x = i * barWidth + barWidth / 2;
            float y = dirtyRect.Height - barHeight - 20;

            canvas.FillRectangle(x, y, barWidth - 10, barHeight);
            canvas.FontSize = 12;
            canvas.FontColor = Colors.Black; // Ensure text is visible
            canvas.DrawString(item.WeekLabel, x, dirtyRect.Height, HorizontalAlignment.Center);
        }
    }


}
