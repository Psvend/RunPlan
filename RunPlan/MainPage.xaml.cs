using RunPlan.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace RunPlan
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        int count = 0;

        public MainPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            LoadActivities();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;
            CounterBtn.Text = count == 1 ? $"Clicked {count} time" : $"Clicked {count} times";
            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private async void LoadActivities()
        {
            List<RunningActivity> activities = await _dbService.GetAllActivitiesAsync();

            string output = "🏃 Running Activities from SQLite:\n";

            foreach (var activity in activities)
            {
                output += $"{activity.Name}, {activity.Distance} km, {activity.Time}\n";
            }

            OutputLabel.Text = output;
        }
    }
}
