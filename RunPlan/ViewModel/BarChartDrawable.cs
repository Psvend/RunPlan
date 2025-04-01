using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BarChartDrawable : IDrawable
{
    public List<RunningDataModel> Data { get; set; } = new();
    public Action<string> OnBarTapped; // 👈 Tooltip callback
    private List<(RectF bounds, RunningDataModel model)> barHitAreas = new(); // Store hit zones

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Data == null || Data.Count == 0)
        {
            Console.WriteLine("No Data Found!");
            return;
        }

        canvas.StrokeColor = Color.FromArgb("7B5EB5");
        canvas.StrokeSize = 1;
        canvas.FontColor = Colors.White; 
        canvas.FontSize = 12;

        float leftMargin = 40;
        float rightMargin = 20;
        float bottomMargin = 70;
        float topMargin = 40;

        float chartWidth = dirtyRect.Width - leftMargin - rightMargin;
        float chartHeight = dirtyRect.Height - bottomMargin - topMargin;
        float maxValue = (float)Math.Ceiling(Data.Max(d => d.Distance));
        if (maxValue <= 0) maxValue = 1;

        float yZero = dirtyRect.Height - bottomMargin;
        float barSpacing = chartWidth / Data.Count;

        // ✅ Y-axis ticks
        int step = 10;
        for (int i = 0; i <= maxValue; i += step)
        {
            float y = yZero - (i / maxValue * chartHeight);
            canvas.DrawLine(leftMargin, y, dirtyRect.Width - rightMargin, y);
            canvas.DrawString($"{i} km", 5, y - 8, HorizontalAlignment.Left);

        }

        Dictionary<string, int> monthFirstIndex = new();
        barHitAreas.Clear(); // ✅ Clear old bar areas

        for (int i = 0; i < Data.Count; i++)
        {
            var item = Data[i];
            float barHeight = (float)(item.Distance / maxValue * chartHeight);
            if (item.Distance == 0.05 && barHeight < 5) barHeight = 5;

            float x = leftMargin + i * barSpacing + barSpacing / 4;
            float barWidth = barSpacing / 2;
            float y = yZero - barHeight;

            canvas.FillColor = Color.FromArgb("4E3689");  //"4C3A70"
            canvas.FillRoundedRectangle(x, y, barWidth, barHeight, 4);

            // ✅ Track clickable bar area
            barHitAreas.Add((new RectF(x, y, barWidth, barHeight), item));

            // ✅ Track first index of each month
            if (!monthFirstIndex.ContainsKey(item.MonthKey))
                monthFirstIndex[item.MonthKey] = i;
        }



        // ✅ Get the latest 3 months from your data
        var lastThreeMonthKeys = Data
            .Select(d => d.MonthKey)
            .Distinct()
            .OrderByDescending(key => key) // Sort newest first
            .Take(3)
            .ToHashSet(); // for fast lookup




        // ✅ Draw Month labels only for the latest 3 months
        foreach (var kvp in monthFirstIndex)
        {
            string monthKey = kvp.Key;
            if (!lastThreeMonthKeys.Contains(monthKey))
                continue; 

            int i = kvp.Value;
            string monthName = DateTime.ParseExact(monthKey, "yyyy-MM", CultureInfo.InvariantCulture)
                                       .ToString("MMMM", CultureInfo.InvariantCulture);

            float x = leftMargin + i * barSpacing + barSpacing / 2;
            canvas.DrawString(monthName, x, yZero + 15, HorizontalAlignment.Center);
        }





        // ✅ Axes
        canvas.StrokeColor = Color.FromArgb("7B5EB5");
        float adjustedTop = topMargin + 40;  //new
        canvas.DrawLine(leftMargin, topMargin, leftMargin, yZero);
        canvas.DrawLine(leftMargin, yZero, dirtyRect.Width - rightMargin, yZero);
    }



    

}





