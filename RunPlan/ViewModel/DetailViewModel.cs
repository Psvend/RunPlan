using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RunPlan.ViewModel;
using Microsoft.Maui.Controls;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Messaging;
using System.Globalization;
using RunPlan.Messages;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;



namespace RunPlan.ViewModel;

[QueryProperty(nameof(RunningActivity), "RunningActivity")]

public partial class DetailViewModel: ObservableObject
{
    private readonly DatabaseService _databaseService;

    public DetailViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;

        //For the time picker
        HourOptions = new ObservableCollection<int>(Enumerable.Range(0, 24));
        MinuteOptions = new ObservableCollection<int>(Enumerable.Range(0, 60));
        SecondOptions = new ObservableCollection<int>(Enumerable.Range(0, 60));
    }

    [ObservableProperty] private RunningActivity runningActivity;
    [ObservableProperty] private bool isEditing;

    // Time picker options
    public ObservableCollection<int> HourOptions { get; }
    public ObservableCollection<int> MinuteOptions { get; }
    public ObservableCollection<int> SecondOptions { get; }

    [ObservableProperty] private int selectedHour;
    [ObservableProperty] private int selectedMinute;
    [ObservableProperty] private int selectedSecond;

    public string CombinedDuration =>
        new TimeSpan(SelectedHour, SelectedMinute, SelectedSecond).ToString(@"hh\:mm\:ss");


    partial void OnRunningActivityChanged(RunningActivity value)
    {
        OnPropertyChanged(nameof(Pace));
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Time));
        OnPropertyChanged(nameof(Distance));
        OnPropertyChanged(nameof(Grade));
        OnPropertyChanged(nameof(Description));

        // Sync pickers with existing time
        if (TimeSpan.TryParse(value.Time, out var parsedTime))
        {
            SelectedHour = parsedTime.Hours;
            SelectedMinute = parsedTime.Minutes;
            SelectedSecond = parsedTime.Seconds;
        }
    }


   



    [RelayCommand]
    public async Task SaveChanges()
    {
        if (RunningActivity != null)
        {
            RunningActivity.Time = CombinedDuration;
            await _databaseService.UpdateActivityAsync(RunningActivity);

            // Reload the updated object from DB
            var updated = await _databaseService.GetActivityByIdAsync(RunningActivity.Id);
            RunningActivity = updated;


            IsEditing = false;
            WeakReferenceMessenger.Default.Send(new ActivityUpdatedMessage());
        }
    }
    


    [RelayCommand]
    public void Edit()
    {
        Console.WriteLine("Edit button clicked!");
        IsEditing = true;
    }


    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => !(bool)value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => !(bool)value;
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
