using RunPlan.Data;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace RunPlan.Model;

public partial class Activity : ContentPage
{

    public List<RunningActivity> Activities { get; set; }

    public Activity(List<RunningActivity> activities)
    {
        InitializeComponent();
        Activities = activities;
        BindingContext = this;

    }


}