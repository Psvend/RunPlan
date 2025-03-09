using RunPlan.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace RunPlan
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public MainPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            LoadActivities();
        }

        private async void LoadActivities()
        {
            var activities = await _dbService.GetAllActivitiesAsync();
            ActivitiesCollectionView.ItemsSource = activities;

            // ✅ Update Last Activity Box
            if (activities.Count > 0)
            {
                var lastActivity = activities[^1]; // Get last activity

                LastActivityDistance.Text = $"{lastActivity.Distance} km";
                LastActivityPace.Text = CalculatePace(lastActivity.Time, lastActivity.Distance);
                LastActivityTime.Text = lastActivity.Time;
            }
            else
            {
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
    }
}





/*

namespace RunPlan
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _dbService;

        public MainPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            LoadActivities();
        }

        private async void LoadActivities()
        {
            var activities = await _dbService.GetAllActivitiesAsync();
            ActivitiesCollectionView.ItemsSource = activities;
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
    }
}

*/












/*
namespace RunPlan
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        
        public MainPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            LoadActivities();
        }





        private async void LoadActivities()
        {
            List<RunningActivity> activities = await _dbService.GetAllActivitiesAsync();

            string output = "🏃 Running Activities from SQLite:\n";

            foreach (var activity in activities)
            {
                output += $"{activity.Name}, {activity.Distance} km, {activity.Time}, {activity.Date}\n";
            }

            OutputLabel.Text = output;
        }
        


        // ✅ Handle adding a new activity
        private async void OnAddActivityClicked(object sender, EventArgs e)
        {
            // Get user input
            string activityName = ActivityNameEntry.Text?.Trim();
            string distanceText = DistanceEntry.Text?.Trim();
            string time = TimeEntry.Text?.Trim();
            string date = DateEntry.Text?.Trim();

            // Validate input
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

            // Insert into database
            await _dbService.InsertRunningActivity(activityName, distance, time, date);

            // Clear input fields
            ActivityNameEntry.Text = "";
            DistanceEntry.Text = "";
            TimeEntry.Text = "";
            DateEntry.Text = "";

            // Refresh activities
            LoadActivities();
        }








    }
}
*/