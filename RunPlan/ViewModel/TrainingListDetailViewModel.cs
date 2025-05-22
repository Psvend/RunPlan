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

        // New: Picker values for hours/minutes/seconds
        public ObservableCollection<int> HourOptions { get; } = new(Enumerable.Range(0, 24));
        public ObservableCollection<int> MinuteOptions { get; } = new(Enumerable.Range(0, 60));
        public ObservableCollection<int> SecondOptions { get; } = new(Enumerable.Range(0, 60));

        [ObservableProperty] private int selectedHour;
        [ObservableProperty] private int selectedMinute;
        [ObservableProperty] private int selectedSecond;

        private readonly DatabaseService _databaseService;

        public ObservableCollection<string> Difficulties { get; } = new()
        {
            "Super Easy", "Easy", "Medium", "Hard", "Extra Hard"
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

                // Split existing time (in minutes) to h/m/s
                var totalTime = TimeSpan.FromMinutes(value.Time);
                SelectedHour = totalTime.Hours;
                SelectedMinute = totalTime.Minutes;
                SelectedSecond = totalTime.Seconds;

                _ = LoadTrainingFields();
            }
        }

        partial void OnIsEditingChanged(bool value)
        {
            if (value && Training != null)
            {
                EditableDistance = Training.Distance;
                SelectedGrade = Training.Grade;

                var totalTime = TimeSpan.FromMinutes(Training.Time);
                SelectedHour = totalTime.Hours;
                SelectedMinute = totalTime.Minutes;
                SelectedSecond = totalTime.Seconds;
            }
        }



        [RelayCommand]
        private async Task Save()
        {
            if (Training != null)
            {
                Training.Distance = (int)EditableDistance;
                Training.Grade = SelectedGrade;

                // Convert duration to total minutes
                var duration = new TimeSpan(SelectedHour, SelectedMinute, SelectedSecond);
                Training.Time = (int)duration.TotalMinutes;

                // Save Training
                await _databaseService.UpdateTrainingAsync(Training);

                // ✅ Save TrainingFields
                for (int i = 0; i < TrainingFields.Count; i++)
                {
                    var field = TrainingFields[i];
                    field.TrainingId = Training.Id;
                    field.SortOrder = i;

                    if (field.Id == 0)
                        await _databaseService.InsertTrainingFieldAsync(field);
                    else
                        await _databaseService.UpdateTrainingFieldAsync(field);
                }

                // ✅ Reload updated training and fields
                var updated = await _databaseService.GetTrainingByIdAsync(Training.Id);
                Training = updated;

                await LoadTrainingFields(); // <--- refreshes UI

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



        [RelayCommand]
        public void AddField()
        {
            if (Training == null)
                return;

            var field = new TrainingField
            {
                TrainingId = Training.Id,
                Text = string.Empty,
                SortOrder = TrainingFields.Count
            };

            TrainingFields.Add(field);
        }

    }
}
