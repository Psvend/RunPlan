
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.Charts;
using RunPlan.Model; 
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


        private async void OnAddActivityClicked(object sender, EventArgs e)
        {
           
            string activityName = ActivityNameEntry.Text?.Trim();
            string distanceText = DistanceEntry.Text?.Trim();
            string time = TimeEntry.Text?.Trim();
            string date = DateEntry.Text?.Trim();

            if (string.IsNullOrEmpty(activityName) || string.IsNullOrEmpty(distanceText) ||
                string.IsNullOrEmpty(time) || string.IsNullOrEmpty(date))
            {
                await DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            if (!double.TryParse(distanceText, out double distance))
            {
                await DisplayAlert("Error", "Distance must be a valid number.", "OK");
                return;
            }

            await ViewModel._dbService.InsertRunningActivity(activityName, distance, time, date);

            ActivityNameEntry.Text = "";
            DistanceEntry.Text = "";
            TimeEntry.Text = "";
            DateEntry.Text = "";

            await ViewModel.LoadActivities(); // Refresh UI
        }


        private async void OnDeleteActivityClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is RunningActivity activity)
            {
                bool confirm = await DisplayAlert("Delete", $"Are you sure you want to delete {activity.Name}?", "Yes", "No");

                if (confirm)
                {
                    await ViewModel._dbService.DeleteActivity(activity.Id);
                    ViewModel.LoadActivities(); // Refresh UI after deletion
                }
            }
        }



        private void OnChartTapped(object sender, TappedEventArgs e)
        {
            var point = e.GetPosition(ChartCanvas);

            if (point == null || ViewModel?.ChartDrawable?.Data == null)
                return;

            float leftMargin = 40;
            float rightMargin = 20;
            float totalWidth = (float)(ChartCanvas.Width - leftMargin - rightMargin);
            int count = ViewModel.ChartDrawable.Data.Count;

            if (count == 0) return;

            float barSpacing = (float)(totalWidth / count);
            int barIndex = (int)((point.Value.X - leftMargin) / barSpacing);



            if (barIndex >= 0 && barIndex < count)
            {
                var item = ViewModel.ChartDrawable.Data[barIndex];

                if (item != null)
                {
                    string distanceLabel = item.Distance == 0.05 ? "0 km" : $"{item.Distance} km";
                    TooltipWeek.Text = item.WeekLabel;
                    TooltipDistance.Text = distanceLabel;
                    TooltipTime.Text = item.Time ?? "00:00:00";
                    TooltipPanel.IsVisible = true;
                }
            }
            else
            {
                TooltipPanel.IsVisible = false;
            }
        }





    }
};










