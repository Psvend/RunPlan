

namespace RunPlan.ViewModel;

[QueryProperty(nameof(RunningActivity), "RunningActivity")]

public partial class DetailViewModel: BaseVievModel
{

    public DetailViewModel() 
    { 
    
    }
    [ObservableProperty]
    RunningActivity runningActivity;

}
