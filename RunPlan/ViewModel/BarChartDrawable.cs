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

        float bottomMargin = 50;
        float barWidth = dirtyRect.Width / (Data.Count + 1);
        float maxHeight = dirtyRect.Height - bottomMargin - 20;
        float maxValue = (float)Data.Max(d => d.Distance);

        Console.WriteLine($"Max Distance Value: {maxValue}");

        if (maxValue <= 0) maxValue = 1;

        for (int i = 0; i < Data.Count; i++)
        {
            var item = Data[i];
            //Console.WriteLine($"📌 Week: {item.WeekLabel}, Distance: {item.Distance}");

            float barHeight = (float)(item.Distance / maxValue * maxHeight);
            float x = i * barWidth + barWidth / 2;
            float y = dirtyRect.Height - barHeight - 25; //edits the space between bottom of bars and labels

            float labelY = y - 20;  //edits the height of the labels 
            canvas.FillRectangle(x, y, barWidth - 40, barHeight);  //edits the thickness of the bars


            canvas.FontSize = 12;
            canvas.FontColor = Colors.Black;

            if (item.Distance > 0) { 
            canvas.DrawString($"{item.Distance} km", x, labelY, HorizontalAlignment.Center);
            }
            //canvas.DrawString(item.WeekLabel, x, labelY, HorizontalAlignment.Center);
        }
    }


}
