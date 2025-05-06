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

        [ObservableProperty]
        private ObservableCollection<TrainingField> trainingFields = new();

        private readonly DatabaseService _databaseService;

        public TrainingListDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

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

        [RelayCommand]
        public async Task LoadTrainingFields()
        {
            if (Training == null)
                return;

            var fields = await _databaseService.GetFieldsForTrainingAsync(Training.Id);
            TrainingFields.Clear();
            foreach (var field in fields.OrderBy(f => f.SortOrder))
            {
                TrainingFields.Add(field);
            }
        }

        // ✅ This triggers LoadTrainingFields when Training is set
        partial void OnTrainingChanged(Training value)
        {
            _ = LoadTrainingFields();
        }
    }
}

