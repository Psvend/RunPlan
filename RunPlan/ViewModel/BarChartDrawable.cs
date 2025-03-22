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

        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 1;
        canvas.FontColor = Colors.Black;
        canvas.FontSize = 12;

        float leftMargin = 40;
        float rightMargin = 20;
        float bottomMargin = 50;
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

            canvas.FillColor = Colors.HotPink;
            canvas.FillRectangle(x, y, barWidth, barHeight);

            // ✅ Track clickable bar area
            barHitAreas.Add((new RectF(x, y, barWidth, barHeight), item));

            // ✅ Track first index of each month
            if (!monthFirstIndex.ContainsKey(item.MonthKey))
                monthFirstIndex[item.MonthKey] = i;
        }

        // ✅ Draw Month labels
        foreach (var kvp in monthFirstIndex)
        {
            int i = kvp.Value;
            string monthKey = kvp.Key;
            string monthName = DateTime.ParseExact(monthKey, "yyyy-MM", CultureInfo.InvariantCulture)
                                       .ToString("MMMM", CultureInfo.InvariantCulture);

            float x = leftMargin + i * barSpacing + barSpacing / 2;
            canvas.DrawString(monthName, x, yZero + 15, HorizontalAlignment.Center);
        }

        // ✅ Axes
        canvas.StrokeColor = Colors.Black;
        canvas.DrawLine(leftMargin, topMargin, leftMargin, yZero);
        canvas.DrawLine(leftMargin, yZero, dirtyRect.Width - rightMargin, yZero);
    }

    // 🔍 Call this from your page when user taps
    public void HandleTap(Point point)
    {
        foreach (var (bounds, model) in barHitAreas)
        {
            if (bounds.Contains((float)point.X, (float)point.Y))
            {
                OnBarTapped?.Invoke($"{model.WeekLabel}: {model.Distance} km");
                return;
            }
        }

        // Tap wasn't on a bar — hide tooltip
        OnBarTapped?.Invoke(null);
    }
}



/*
public class BarChartDrawable : IDrawable
{
    public List<RunningDataModel> Data { get; set; } = new();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Data == null || Data.Count == 0)
        {
            Console.WriteLine("No Data Found!");
            return;
        }

        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 1;
        canvas.FontColor = Colors.Black;
        canvas.FontSize = 12;

        float leftMargin = 40;
        float rightMargin = 20;
        float bottomMargin = 50;
        float topMargin = 40;

        float chartWidth = dirtyRect.Width - leftMargin - rightMargin;
        float chartHeight = dirtyRect.Height - bottomMargin - topMargin;
        float maxValue = (float)Math.Ceiling(Data.Max(d => d.Distance));
        if (maxValue <= 0) maxValue = 1;

        float yZero = dirtyRect.Height - bottomMargin;
        float barSpacing = chartWidth / Data.Count;

        // ✅ Draw Y-axis ticks
        int step = 10;
        for (int i = 0; i <= maxValue; i += step)
        {
            float y = yZero - (i / maxValue * chartHeight);
            canvas.DrawLine(leftMargin, y, dirtyRect.Width - rightMargin, y);
            canvas.DrawString($"{i} km", 5, y - 8, HorizontalAlignment.Left);
        }

        // ✅ Track where each month starts
        Dictionary<string, int> monthFirstIndex = new();

        for (int i = 0; i < Data.Count; i++)
        {
            var item = Data[i];
            float barHeight = (float)(item.Distance / maxValue * chartHeight);
            if (item.Distance == 0.05 && barHeight < 5) barHeight = 5;

            float x = leftMargin + i * barSpacing + barSpacing / 4;
            float barWidth = barSpacing / 2;
            float y = yZero - barHeight;

            canvas.FillColor = Colors.HotPink;
            canvas.FillRectangle(x, y, barWidth, barHeight);

            if (item.Distance > 0.05)
                canvas.DrawString($"{item.Distance} km", x + barWidth / 2, y - 15, HorizontalAlignment.Center);

            // ✅ Use activity monthKey, not weekStart month
            if (!monthFirstIndex.ContainsKey(item.MonthKey))
                monthFirstIndex[item.MonthKey] = i;
        }

        // ✅ Draw Month labels (use correct monthKey names)
        foreach (var kvp in monthFirstIndex)
        {
            int i = kvp.Value;
            string monthKey = kvp.Key;
            string monthName = DateTime.ParseExact(monthKey, "yyyy-MM", CultureInfo.InvariantCulture)
                                       .ToString("MMMM", CultureInfo.InvariantCulture);

            float x = leftMargin + i * barSpacing + barSpacing / 2;
            canvas.DrawString(monthName, x, yZero + 15, HorizontalAlignment.Center);
        }

        // ✅ Draw axes
        canvas.StrokeColor = Colors.Black;
        canvas.DrawLine(leftMargin, topMargin, leftMargin, yZero);
        canvas.DrawLine(leftMargin, yZero, dirtyRect.Width - rightMargin, yZero);
    }
}

*/