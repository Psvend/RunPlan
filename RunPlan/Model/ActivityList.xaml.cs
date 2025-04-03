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

    



}
