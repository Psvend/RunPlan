using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RunPlan.Data;
using Microsoft.Maui.Controls;

namespace RunPlan.Model;

[QueryProperty(nameof(RunningActivity), "RunningActivity")]
public partial class ActivityDetail : ContentPage
{
    private RunningActivity _activity;

    public ActivityDetail()
    {
        InitializeComponent();
    }

    public RunningActivity RunningActivity
    {
        get => _activity;
        set
        {
            _activity = value;
            BindingContext = _activity;
        }
    }

    //Handles back button
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ActivityList");
    }


}




