using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using RunPlan.Model;

namespace RunPlan.ViewModel
{
    [QueryProperty(nameof(Training), "Training")]
    public partial class TrainingListDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private Training training;
        [ObservableProperty]
        private bool isEditing;

        [RelayCommand]
        private void Edit()
        {
            IsEditing = true;
        }
        [RelayCommand]
        private async Task Save()
        {
            IsEditing = false;
        }
        
    }

}
