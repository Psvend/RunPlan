using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RunPlan.ViewModel;
using Microsoft.Maui.Controls;



namespace RunPlan.Model;

public partial class ActivityList : ContentPage
{

    private bool isNavigating = false; 


    public ActivityList(ActivityListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private async void OnAddActivityClicked(object sender, EventArgs e)
    {
        if (isNavigating) return;
        isNavigating = true;

        await Shell.Current.GoToAsync("//Activity");

        isNavigating = false;
    }


    //Refreshes ActivityList page after coming back from add activity page
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ActivityListViewModel vm)
            await vm.LoadActivities();
    }


    //Search bar 
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is ActivityListViewModel vm)
        {
            vm.FilterActivitiesBySearch();
        }
    }

    private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }


    //Navigates to activity details page when an activity is selected
    private async void OnActivitySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is RunningActivity selected)
        {
            await Shell.Current.GoToAsync("ActivityDetail", true, new Dictionary<string, object>
        {
            { "RunningActivity", selected }
        });

            ((CollectionView)sender).SelectedItem = null; // clear selection
        }
    }







}
