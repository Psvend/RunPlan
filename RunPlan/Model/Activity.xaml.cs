using RunPlan.Data;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace RunPlan.Model;

public partial class Activity : ContentPage
{


    public Activity(ActivityViewModel activityViewModel)
    {
        InitializeComponent();
        BindingContext = activityViewModel;

    }


}