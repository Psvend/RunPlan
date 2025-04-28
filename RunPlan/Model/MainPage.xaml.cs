
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
using CommunityToolkit.Mvvm.Messaging;
using RunPlan.Messages;
using CommunityToolkit.Mvvm.Input;



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


            //Listens after updates from other pages
            WeakReferenceMessenger.Default.Register<ActivityUpdatedMessage>(this, async (r, m) =>
            {
                await ViewModel.LoadActivities(); // or a smarter refresh
                UpdateLastActivityUI(ViewModel.LastActivity);
            });



            UpdateLastActivityUI(ViewModel.LastActivity);



            //Forces redraw when graph updates
            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.ChartDrawable))
                {
                    ChartCanvas.Invalidate(); // 👈 Triggers chart redraw!
                }
            };




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


        //To update Last Activity box and This Week box
        private void UpdateLastActivityUI(RunningActivity activity)
        {
            if (activity == null) return;

            LastActivityName.Text = activity.Name;
            LastActivityDistance.Text = $"{activity.Distance} km";

            if (TimeSpan.TryParse(activity.Time, out TimeSpan parsedTime))
            {
                LastActivityTime.Text = $"{parsedTime.Hours}h{parsedTime.Minutes}min";
            }
            else
            {
                LastActivityTime.Text = activity.Time; // fallback
            }

            LastActivityPace.Text = MainViewModel.CalculatePace(activity.Time, activity.Distance);
        }


        //To help navigation between pages to go smoother
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
           // MessageCenter.Unsubscribe<ActivityViewModel>(this, "ActivityUpdated");
        }



    }
};










