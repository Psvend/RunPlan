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





