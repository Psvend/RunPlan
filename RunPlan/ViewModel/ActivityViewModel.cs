using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunPlan.ViewModel;

public partial class ActivityViewModel : BaseVievModel
{
    public ObservableCollection<RunningActivity> RunningActivities { get; } = new();
    readonly DatabaseService _databaseService;

    public ActivityViewModel(DatabaseService databaseService)
    {
        this._databaseService = databaseService;
        Title = "Running Activities";
    }
    [RelayCommand]
    public async Task LoadActivities()
    {

        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            
            var activities = await _databaseService.GetAllActivitiesAsync();




            foreach (var activity in activities)
            {
                RunningActivities.Add(activity);


            }
        }
        finally
        {
            IsBusy = false;
        }
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


}

