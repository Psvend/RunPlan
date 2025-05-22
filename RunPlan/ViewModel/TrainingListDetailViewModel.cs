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
        
        [ObservableProperty]
        private string selectedGrade;

        [ObservableProperty]
        private double editableDistance;

        private readonly DatabaseService _databaseService;


        public ObservableCollection<string> Difficulties { get; } = new()
        {
        "Super Easy", "Easy", "Medium", "Hard","Extra Hard"
        };

        public TrainingListDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }



        [RelayCommand]
        private void Edit()
        {
            IsEditing = true;
        }

        partial void OnTrainingChanged(Training value)
        {
            if (value != null)
            {
                EditableDistance = value.Distance;
                SelectedGrade = value.Grade;
                _ = LoadTrainingFields();
            }
        }

        partial void OnIsEditingChanged(bool value)
        {
            if (value && Training != null)
            {
                EditableDistance = Training.Distance;
                SelectedGrade = Training.Grade;
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (Training != null)
            {
                Training.Distance = (int)EditableDistance;
                Training.Grade = SelectedGrade; // Apply edited value
                await _databaseService.UpdateTrainingAsync(Training); // Save to DB

                // ✅ Re-fetch updated data from DB
                var updated = await _databaseService.GetTrainingByIdAsync(Training.Id);
                Training = updated; // ✅ Triggers OnTrainingChanged and UI update


                IsEditing = false;
            }
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



        
    }
}

