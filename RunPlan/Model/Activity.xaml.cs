using RunPlan.Data;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using RunPlan.ViewModel;
using CommunityToolkit.Mvvm.Messaging;
using RunPlan.Messages;



namespace RunPlan.Model;

public partial class Activity : ContentPage
{
    private readonly ActivityViewModel _viewModel;

    public Activity(ActivityViewModel activityViewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = activityViewModel;

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.RunningActivities))
            {
                WeakReferenceMessenger.Default.Send(new ActivityUpdatedMessage());
            }
        };
    
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Optional cleanup
        WeakReferenceMessenger.Default.Unregister<ActivityUpdatedMessage>(this);

    }


    //Handles back button navigation to return to Activity List
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ActivityList"); // Goes back one page
    }


}







/*
 * CODE FROM MAINPAGE


        private async void OnDeleteActivityClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is RunningActivity activity)
            {
                bool confirm = await DisplayAlert("Delete", $"Are you sure you want to delete {activity.Name}?", "Yes", "No");

                if (confirm)
                {
                    await ViewModel._dbService.DeleteActivity(activity.Id);
                    ViewModel.LoadActivities(); // Refresh UI after deletion
                    UpdateLastActivityUI(ViewModel.LastActivity);
                }
            }
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
            UpdateLastActivityUI(ViewModel.LastActivity);
        }

 */