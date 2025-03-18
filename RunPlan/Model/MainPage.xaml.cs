
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.Charts;
using RunPlan.Model; // Import RunningDataModel
using RunPlan.ViewModel;



namespace RunPlan.Model
{
    public partial class MainPage : ContentPage
    {
        public MainViewModel ViewModel { get; private set; }

        public MainPage(DatabaseService dbService)
        {
            InitializeComponent();
            ViewModel = new MainViewModel(dbService);
            BindingContext = ViewModel;
        }
    }
};






/*

namespace RunPlan.Model
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<RunningDataModel> WeeklyRunningData { get; set; }
        public BarChartDrawable ChartDrawable { get; set; }
        private readonly DatabaseService _dbService;
        private Dictionary<string, List<RunningDataModel>> monthlyData;

        public MainPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            BindingContext = this;
            WeeklyRunningData = new ObservableCollection<RunningDataModel>();
            ChartDrawable = new BarChartDrawable { Data = WeeklyRunningData.ToList() };
            ChartCanvas.BindingContext = this;

            LoadActivities();
        }

        private async void LoadActivities()
        {
            var activities = await _dbService.GetAllActivitiesAsync();

            DateTime today = DateTime.Now;
            DateTime firstDayOfThisMonth = new DateTime(today.Year, today.Month, 1);
            DateTime firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);
            DateTime firstDayOfTwoMonthsAgo = firstDayOfThisMonth.AddMonths(-2);

            var filteredActivities = activities
                .Where(a => DateTime.TryParseExact(a.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                                   DateTimeStyles.None, out DateTime parsedDate)
                            && parsedDate >= firstDayOfTwoMonthsAgo)
                .ToList();

            monthlyData = new Dictionary<string, List<RunningDataModel>>();
            CultureInfo culture = CultureInfo.InvariantCulture;
            Calendar calendar = culture.Calendar;

            foreach (var activity in filteredActivities)
            {
                if (DateTime.TryParseExact(activity.Date, "yyyy-MM-dd", culture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    string monthKey = parsedDate.ToString("yyyy-MM");
                    int weekNumber = calendar.GetWeekOfYear(parsedDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                    if (!monthlyData.ContainsKey(monthKey))
                        monthlyData[monthKey] = new List<RunningDataModel>();

                    var weekData = monthlyData[monthKey].FirstOrDefault(w => w.WeekLabel == $"Week {weekNumber}");
                    if (weekData == null)
                    {
                        monthlyData[monthKey].Add(new RunningDataModel
                        {
                            WeekLabel = $"Week {weekNumber}",
                            Distance = activity.Distance
                        });
                    }
                    else
                    {
                        weekData.Distance += activity.Distance;
                    }
                }
            }

            var currentMonth = today.ToString("yyyy-MM");
            UpdateChartForMonth(currentMonth);
        }

        private void UpdateChartForMonth(string selectedMonth)
        {
            if (monthlyData.ContainsKey(selectedMonth))
            {
                var selectedData = monthlyData[selectedMonth].OrderBy(d => d.WeekLabel).ToList();

                WeeklyRunningData.Clear();
                foreach (var item in selectedData)
                {
                    WeeklyRunningData.Add(item);
                }

                ChartDrawable.Data = selectedData;
                ChartCanvas.Invalidate();
            }
        }

        private void OnMonthChanged(object sender, EventArgs e)
        {
            if (MonthPicker.SelectedItem == null) return;
            string selectedMonth = MonthPicker.SelectedItem.ToString();
            UpdateChartForMonth(selectedMonth);
        }

        // ✅ Handle adding a new activity
        private async void OnAddActivityClicked(object sender, EventArgs e)
        {
            Console.WriteLine("Add Activity Button Clicked!");

            string activityName = ActivityNameEntry.Text?.Trim();
            string distanceText = DistanceEntry.Text?.Trim();
            string time = TimeEntry.Text?.Trim();
            string date = DateEntry.Text?.Trim();

            if (string.IsNullOrEmpty(activityName) || string.IsNullOrEmpty(distanceText) || string.IsNullOrEmpty(time) || string.IsNullOrEmpty(date))
            {
                await DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            if (!double.TryParse(distanceText, out double distance))
            {
                await DisplayAlert("Error", "Distance must be a valid number.", "OK");
                return;
            }

            await _dbService.InsertRunningActivity(activityName, distance, time, date);

            ActivityNameEntry.Text = "";
            DistanceEntry.Text = "";
            TimeEntry.Text = "";
            DateEntry.Text = "";

            Console.WriteLine("New Activity Added! Refreshing Chart...");
            LoadActivities(); // Refresh UI
        }

        // ✅ Handle deleting an activity
        private async void OnDeleteActivityClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is RunningActivity activity)
            {
                bool confirm = await DisplayAlert("Delete", $"Are you sure you want to delete {activity.Name}?", "Yes", "No");

                if (confirm)
                {
                    await _dbService.DeleteActivity(activity.Id);
                    LoadActivities(); // Refresh UI after deletion
                }
            }
        }

        // ✅ Calculate Running Pace (min/km)
        private string CalculatePace(string time, double distance)
        {
            try
            {
                string[] parts = time.Split(':'); // Expecting format "hh:mm:ss"
                if (parts.Length != 3) return "N/A";

                int hours = int.Parse(parts[0]);
                int minutes = int.Parse(parts[1]);
                int seconds = int.Parse(parts[2]);

                double totalMinutes = hours * 60 + minutes + (seconds / 60.0);
                double pace = totalMinutes / distance;

                int paceMinutes = (int)pace;
                int paceSeconds = (int)((pace - paceMinutes) * 60);

                return $"{paceMinutes}:{paceSeconds:00} min/km";
            }
            catch
            {
                return "N/A"; // Return default if calculation fails
            }
        }

        private void UpdateWeeklyStats(List<RunningActivity> recentActivities)
        {
            double totalDistance = recentActivities.Sum(a => a.Distance);
            TimeSpan totalTime = TimeSpan.Zero;

            foreach (var activity in recentActivities)
            {
                totalTime += ParseTime(activity.Time);
            }

            // ✅ Update UI Elements
            WeekActivityHours.Text = $"{FormatTimeSpan(totalTime)}";
            WeekActivityDistance.Text = $"{totalDistance} km";
        }

        // ✅ Parses "hh:mm:ss" format and returns a TimeSpan
        private TimeSpan ParseTime(string time)
        {
            try
            {
                string[] parts = time.Split(':');
                if (parts.Length != 3) return TimeSpan.Zero;

                int hours = int.Parse(parts[0]);
                int minutes = int.Parse(parts[1]);
                int seconds = int.Parse(parts[2]);

                return new TimeSpan(hours, minutes, seconds);
            }
            catch
            {
                return TimeSpan.Zero; // Default if parsing fails
            }
        }

        // ✅ Converts TimeSpan to a readable format like "6h10min"
        private string FormatTimeSpan(TimeSpan time)
        {
            return $"{(int)time.TotalHours}h{time.Minutes}min";
        }
    }
}


*/









/*


namespace RunPlan.Model
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<RunningDataModel> WeeklyRunningData { get; set; }
        public BarChartDrawable ChartDrawable { get; set; }
        private readonly DatabaseService _dbService;

        public MainPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            BindingContext = this;
            WeeklyRunningData = new ObservableCollection<RunningDataModel>();
            ChartDrawable = new BarChartDrawable { Data = WeeklyRunningData.ToList() };
            ChartCanvas.BindingContext = this;
            
            LoadActivities();
        }

        private async void LoadActivities()
        {
            var activities = await _dbService.GetAllActivitiesAsync();

            DateTime today = DateTime.Now;
            DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var calendar = CultureInfo.InvariantCulture.Calendar;

            var filteredActivities = activities
                .Where(a => DateTime.TryParseExact(
                    a.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime parsedDate)
                    && parsedDate >= firstDayOfMonth && parsedDate <= lastDayOfMonth)
                .ToList();

            // ✅ Generate Weekly Running Data (Ensure No Weeks Are Skipped)
            Dictionary<int, double> weeklyData = new Dictionary<int, double>();

            // Step 1: Initialize all weeks in the current month with 0 km
            for (DateTime weekStart = firstDayOfMonth; weekStart <= lastDayOfMonth; weekStart = weekStart.AddDays(7))
            {
                int weekNumber = calendar.GetWeekOfYear(weekStart, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                if (!weeklyData.ContainsKey(weekNumber))
                    weeklyData[weekNumber] = 0; // Initialize with 0 km
            }

            // Step 2: Aggregate activity distances per week
            foreach (var activity in filteredActivities)
            {
                if (DateTime.TryParseExact(activity.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime parsedDate))
                {
                    int weekNumber = calendar.GetWeekOfYear(parsedDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    weeklyData[weekNumber] += activity.Distance;
                }
            }

            // Step 3: Convert dictionary to List for Chart Binding
            var groupedData = weeklyData
                .OrderBy(kvp => kvp.Key) // Order by week number
                .Select(kvp => new RunningDataModel
                {
                    WeekLabel = $"Week {kvp.Key}",
                    Distance = kvp.Value
                })
                .ToList();


            // ✅ Prevent graph crashes by ensuring it's never null
            if (!groupedData.Any())
            {
                groupedData = new List<RunningDataModel>
                {
                    new RunningDataModel { WeekLabel = "No Data", Distance = 0 }
                };
            }

            // ✅ Update WeeklyRunningData (Ensure UI is refreshed)
            WeeklyRunningData.Clear();
            foreach (var item in groupedData)
            {
                WeeklyRunningData.Add(item);
            }

            // ✅ Update ChartDrawable & Refresh UI
            ChartDrawable.Data = groupedData.ToList();
            ChartCanvas.Invalidate(); // Force redraw of the chart




            // ✅ Ensure WeeklyDistanceChart is found before binding
            var chart = this.FindByName<SfCartesianChart>("WeeklyDistanceChart");
            if (chart != null)
            {
                chart.BindingContext = new { WeeklyRunningData = groupedData };
            }

            ActivitiesCollectionView.ItemsSource = activities;

            // ✅ Update Last Activity Box (Safe from crashes)
            if (activities.Any())
            {
                var lastActivity = activities.Last();
                LastActivityName.Text = lastActivity.Name;
                LastActivityDistance.Text = $"{lastActivity.Distance} km";
                LastActivityPace.Text = CalculatePace(lastActivity.Time, lastActivity.Distance);
                LastActivityTime.Text = lastActivity.Time;
            }
            else
            {
                LastActivityName.Text = "N/A";
                LastActivityDistance.Text = "N/A";
                LastActivityPace.Text = "N/A";
                LastActivityTime.Text = "N/A";
            }

            
        }




        // ✅ Handle adding a new activity
        private async void OnAddActivityClicked(object sender, EventArgs e)
        {
            string activityName = ActivityNameEntry.Text?.Trim();
            string distanceText = DistanceEntry.Text?.Trim();
            string time = TimeEntry.Text?.Trim();
            string date = DateEntry.Text?.Trim();

            if (string.IsNullOrEmpty(activityName) || string.IsNullOrEmpty(distanceText) || string.IsNullOrEmpty(time) || string.IsNullOrEmpty(date))
            {
                await DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            if (!double.TryParse(distanceText, out double distance))
            {
                await DisplayAlert("Error", "Distance must be a number.", "OK");
                return;
            }

            await _dbService.InsertRunningActivity(activityName, distance, time, date);

            ActivityNameEntry.Text = "";
            DistanceEntry.Text = "";
            TimeEntry.Text = "";
            DateEntry.Text = "";

            Console.WriteLine(" New Activity Added! Refreshing Chart...");
            LoadActivities(); // Refresh UI
        }

        // ✅ Handle deleting an activity
        private async void OnDeleteActivityClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is RunningActivity activity)
            {
                bool confirm = await DisplayAlert("Delete", $"Are you sure you want to delete {activity.Name}?", "Yes", "No");

                if (confirm)
                {
                    await _dbService.DeleteActivity(activity.Id);
                    LoadActivities(); // Refresh UI after deletion
                }
            }
        }

        // ✅ Calculate Running Pace (min/km)
        private string CalculatePace(string time, double distance)
        {
            try
            {
                string[] parts = time.Split(':'); // Expecting format "hh:mm:ss"
                if (parts.Length != 3) return "N/A";

                int hours = int.Parse(parts[0]);
                int minutes = int.Parse(parts[1]);
                int seconds = int.Parse(parts[2]);

                double totalMinutes = hours * 60 + minutes + (seconds / 60.0);
                double pace = totalMinutes / distance;

                int paceMinutes = (int)pace;
                int paceSeconds = (int)((pace - paceMinutes) * 60);

                return $"{paceMinutes}:{paceSeconds:00} min/km";
            }
            catch
            {
                return "N/A"; // Return default if calculation fails
            }
        }

        private void UpdateWeeklyStats(List<RunningActivity> recentActivities)
        {
            double totalDistance = recentActivities.Sum(a => a.Distance);
            TimeSpan totalTime = TimeSpan.Zero;

            foreach (var activity in recentActivities)
            {
                totalTime += ParseTime(activity.Time);
            }

            // ✅ Update UI Elements
            WeekActivityHours.Text = $"{FormatTimeSpan(totalTime)}";
            WeekActivityDistance.Text = $"{totalDistance} km";
        }

        // ✅ Parses "hh:mm:ss" format and returns a TimeSpan
        private TimeSpan ParseTime(string time)
        {
            try
            {
                string[] parts = time.Split(':');
                if (parts.Length != 3) return TimeSpan.Zero;

                int hours = int.Parse(parts[0]);
                int minutes = int.Parse(parts[1]);
                int seconds = int.Parse(parts[2]);

                return new TimeSpan(hours, minutes, seconds);
            }
            catch
            {
                return TimeSpan.Zero; // Default if parsing fails
            }
        }

        // Converts TimeSpan to a readable format like "6h10min"
        private string FormatTimeSpan(TimeSpan time)
        {
            return $"{(int)time.TotalHours}h{time.Minutes}min";
        }
    }



}


*/
