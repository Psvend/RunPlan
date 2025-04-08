using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RunPlan.Data;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RunPlan.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;





namespace RunPlan.ViewModel;

public partial class ActivityListViewModel : BaseVievModel
{
    public ObservableCollection<RunningActivity> RunningActivities { get; } = new();

    private readonly DatabaseService _databaseService;
    private List<RunningActivity> allActivities = new(); // Store all for reuse





    public ActivityListViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Running Activities";

        // Initial load
        _ = LoadActivities();
    }

    // 🟣 Input-bound properties
    [ObservableProperty] private string activityName;
    [ObservableProperty] private string distanceText;
    [ObservableProperty] private string time;
    [ObservableProperty] private string date;
    [ObservableProperty] private bool isEmptyMessageVisible;
    [ObservableProperty] private List<int> availableYears;
    [ObservableProperty] private int selectedYear;
    [ObservableProperty] private string searchQuery;


    [RelayCommand]
    public async Task LoadActivities()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            RunningActivities.Clear();

            // Get all activities
            allActivities = await _databaseService.GetAllActivitiesAsync();

            // 🔁 Set AvailableYears from activities
            AvailableYears = allActivities
                .Select(a => DateTime.TryParse(a.Date, out var date) ? date.Year : 0)
                .Where(y => y > 0)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            SelectedYear = DateTime.Now.Year; // Default to current year

            // 🔁 Set AvailableMonths to ALL months of the current year
            AvailableMonths.Clear();
            var allMonthNames = DateTimeFormatInfo.InvariantInfo.MonthNames
                                    .Where(m => !string.IsNullOrEmpty(m)) // Avoid empty at end
                                    .ToList();

            foreach (var month in allMonthNames)
            {
                AvailableMonths.Add(month);
            }

            SelectedMonth = DateTime.Now.ToString("MMMM"); // Default to current month
        }
        finally
        {
            IsBusy = false;
        }
    }




    [RelayCommand]
    public async Task AddActivity()
    {
        if (string.IsNullOrWhiteSpace(ActivityName) ||
            string.IsNullOrWhiteSpace(DistanceText) ||
            string.IsNullOrWhiteSpace(Time) ||
            string.IsNullOrWhiteSpace(Date))
        {
            await Shell.Current.DisplayAlert("Error", "Please fill in all fields.", "OK");
            return;
        }

        if (!double.TryParse(DistanceText, out double distance))
        {
            await Shell.Current.DisplayAlert("Error", "Distance must be a valid number.", "OK");
            return;
        }

        await _databaseService.InsertRunningActivity(ActivityName, distance, Time, Date);

        // Clear inputs
        ActivityName = DistanceText = Time = Date = string.Empty;

        await LoadActivities();

        // Notify listeners (like MainPage) if necessary
        WeakReferenceMessenger.Default.Send(new ActivityUpdatedMessage());
    }


    [RelayCommand]
    public async Task DeleteActivity(RunningActivity activity)
    {
        if (activity == null) return;

        bool confirm = await Shell.Current.DisplayAlert("Delete", $"Are you sure you want to delete {activity.Name}?", "Yes", "No");
        if (!confirm) return;

        await _databaseService.DeleteActivity(activity.Id);
        await LoadActivities();

        WeakReferenceMessenger.Default.Send(new ActivityUpdatedMessage());
    }



    [RelayCommand]
    public async Task GoToDetails(RunningActivity activity)
    {
        if (activity == null) return;

        await Shell.Current.GoToAsync(nameof(DetailScreen), true, new Dictionary<string, object>
        {
            { "RunningActivity", activity }
        });
    }



    //Week filter
    [RelayCommand]
    private void FilterThisWeek()
    {
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));

        var filtered = allActivities
            .Where(a => DateTime.TryParse(a.Date, out var date) && date >= startOfWeek)
            .OrderByDescending(a => a.Date);

        ApplyFilter(filtered);
    }





    //Year filter

    [RelayCommand]
    private void FilterThisYear()
    {
        var now = DateTime.Today;

        var filtered = allActivities
            .Where(a =>
                DateTime.TryParse(a.Date, out var date) &&
                date.Year == now.Year)
            .OrderByDescending(a => date)
            .ToList();

        IsEmptyMessageVisible = filtered.Count == 0;

        ApplyFilter(filtered);
    }

    partial void OnSelectedYearChanged(int value)
    {
        FilterByYearAndMonth(value, SelectedMonth);
    }

    private void FilterByYear(int year)
    {
        var filtered = allActivities
            .Where(a => DateTime.TryParse(a.Date, out var date) && date.Year == year)
            .OrderByDescending(a => date)
            .ToList();

        IsEmptyMessageVisible = filtered.Count == 0;
        ApplyFilter(filtered);
    }


    //BE SURE THAT THE YEAR FILTERS AFTER THE MONTH HAS BEEN PICKED. NEED TO BE CONNECTED. DEFAULT SHOULD BE CURRENT MONTH

    //Month filter

    [RelayCommand]
    private void FilterThisMonth()
    {
        var now = DateTime.Today;

        var filtered = allActivities
            .Where(a =>
                DateTime.TryParse(a.Date, out var date) &&
                date.Month == now.Month &&
                date.Year == now.Year)
            .OrderByDescending(a => date)
            .ToList();

        IsEmptyMessageVisible = filtered.Count == 0;

        ApplyFilter(filtered);
    }

    private void ApplyFilter(IEnumerable<RunningActivity> filtered)
    {
        RunningActivities.Clear();
        foreach (var act in filtered)
            RunningActivities.Add(act);
    }


    public ObservableCollection<string> AvailableMonths { get; } = new();
    [ObservableProperty] private string selectedMonth;


    //To help the sort month picker
    partial void OnSelectedMonthChanged(string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        FilterByYearAndMonth(SelectedYear, value);
    }




    private void ApplyFilter(List<RunningActivity> filtered)
    {
        RunningActivities.Clear();
        foreach (var a in filtered)
            RunningActivities.Add(a);
    }



    private void FilterByYearAndMonth(int year, string month)
    {
        var filtered = allActivities
            .Where(a =>
                DateTime.TryParse(a.Date, out var d) &&
                d.Year == year &&
                d.ToString("MMMM", CultureInfo.InvariantCulture) == month)
            .Select(a =>
            {
                DateTime.TryParse(a.Date, out var parsedDate);
                return new { Activity = a, ParsedDate = parsedDate };
            })
            .OrderByDescending(x => x.ParsedDate)
            .Select(x => x.Activity)
            .ToList();

        IsEmptyMessageVisible = filtered.Count == 0;

        ApplyFilter(filtered);
    }



    //To handle search bar
    public void FilterActivitiesBySearch()
    {
        IEnumerable<RunningActivity> filtered = allActivities;

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            filtered = filtered
                .Where(a => a.Name != null &&
                            a.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
        }

        ApplyFilter(filtered);
    }




}
