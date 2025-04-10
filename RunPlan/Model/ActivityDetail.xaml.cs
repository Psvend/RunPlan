using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RunPlan.Data;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using Syncfusion.Maui.Core.Carousel;


namespace RunPlan.Model;

[QueryProperty(nameof(RunningActivity), "RunningActivity")]
public partial class ActivityDetail : ContentPage, INotifyPropertyChanged
{
    private RunningActivity _activity;
    public event PropertyChangedEventHandler PropertyChanged;
    public DetailViewModel ViewModel { get; }
    
    public ActivityDetail(DetailViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;
    }



    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));



    public RunningActivity RunningActivity
    {
        get => ViewModel.RunningActivity;
        set => ViewModel.RunningActivity = value;
    }



    //Handles back button
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ActivityList");
    }


}




