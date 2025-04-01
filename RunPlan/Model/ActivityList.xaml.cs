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
    public ActivityList(ActivityViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private async void OnAddActivityClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Activity");
    }
}
