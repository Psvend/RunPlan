using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RunPlan.ViewModel;
using Microsoft.Maui.Controls;
using System.Xml.Linq;



namespace RunPlan.ViewModel;

[QueryProperty(nameof(RunningActivity), "RunningActivity")]

public partial class DetailViewModel: BaseVievModel
{


    [ObservableProperty]
    private RunningActivity runningActivity;

    partial void OnRunningActivityChanged(RunningActivity value)
    {
        OnPropertyChanged(nameof(Pace));
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Time));
        OnPropertyChanged(nameof(Distance));
        OnPropertyChanged(nameof(Grade));
        OnPropertyChanged(nameof(Description));
    }

    public string Name => RunningActivity?.Name ?? "N/A";
    public string Time => RunningActivity?.Time ?? "00:00:00";
    public double Distance => RunningActivity?.Distance ?? 0;
    public string Grade => RunningActivity?.Grade ?? "Empty for now";
    public string Description => RunningActivity?.Description ?? "Nothing yet";





    public string Pace
    {
        get
        {
            if (TimeSpan.TryParse(RunningActivity?.Time, out var time) &&
                double.TryParse(RunningActivity?.Distance.ToString(), out var distance) &&
                distance > 0)
            {
                double minutesPerKm = time.TotalMinutes / distance;
                return $"{minutesPerKm:0.00}";
            }

            return "N/A";
        }
    }






}
