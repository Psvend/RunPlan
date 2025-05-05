using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunPlan.Data;
using RunPlan.Model;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using RunPlan.Messages;



namespace RunPlan.ViewModel
{
    public partial class CreateTrainingViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private List<Training> allTrainings = new();

        public CreateTrainingViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _ = LoadTrainingsAsync();
            Task.Run(async () => await LoadTrainingsAsync());
        }

        // Form inputs
        [ObservableProperty] private string name;
        [ObservableProperty] private string description;
        [ObservableProperty] private string time;
        [ObservableProperty] private string grade;
        [ObservableProperty] private string distance;

        [ObservableProperty] private ObservableCollection<Training> existingTrainings = new();
        [ObservableProperty] private bool isBusy;

        [ObservableProperty]
        private List<string> availableGrades = new()
        {
            "Super Easy", "Easy", "Medium", "Hard", "Extra Hard"
        };

        [ObservableProperty] private ObservableCollection<TrainingField> customFields = new();
        [ObservableProperty] private Training currentTraining;
       
        const int MaxCustomFields = 10;




        [RelayCommand]
        public async Task LoadTrainingsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                allTrainings = await _databaseService.GetAllTrainingsAsync();
                ExistingTrainings.Clear();

                foreach (var t in allTrainings)
                    ExistingTrainings.Add(t);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to load trainings: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ✅ Refactored SaveTraining method using composite DB transaction
        [RelayCommand]
        public async Task SaveTraining()
        {
            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Time) ||
                string.IsNullOrWhiteSpace(Grade) ||
                string.IsNullOrWhiteSpace(Description) ||
                string.IsNullOrWhiteSpace(Distance))
            {
                await Shell.Current.DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            if (!int.TryParse(Time, out int timeInt) || !int.TryParse(Distance, out int distanceInt))
            {
                await Shell.Current.DisplayAlert("Error", "Time and Distance must be valid numbers.", "OK");
                return;
            }

            var training = new Training
            {
                Name = Name,
                Description = Description,
                Time = timeInt,
                Grade = Grade,
                Distance = distanceInt
            };

            var fieldList = CustomFields.ToList();

            int newId = await _databaseService.InsertTrainingWithFieldsAsync(training, fieldList);
            training.Id = newId;
            CurrentTraining = training;

            // Clear inputs
            Name = Description = Time = Grade = Distance = string.Empty;
            CustomFields.Clear();

            await LoadTrainingsAsync();

            WeakReferenceMessenger.Default.Send(new TrainingUpdatedMessage());
        }

        [RelayCommand]
        public async Task LoadCustomFields(int trainingId)
        {
            var fields = await _databaseService.GetFieldsForTrainingAsync(trainingId);
            CustomFields.Clear();
            foreach (var f in fields)
                CustomFields.Add(f);
        }

        [RelayCommand]
        public void AddField()
        {
            if (CustomFields.Count >= MaxCustomFields)
                return;

            var nextIndex = CustomFields.Count;

            var field = new TrainingField
            {
                TrainingId = 0, // Temporary until saved
                Text = string.Empty,
                SortOrder = nextIndex
            };

            CustomFields.Add(field);
        }

        [RelayCommand]
        public async Task SaveCustomFields()
        {
            for (int i = 0; i < CustomFields.Count; i++)
            {
                var f = CustomFields[i];
                f.SortOrder = i;
                if (f.Id == 0)
                    await _databaseService.InsertTrainingFieldAsync(f);
                else
                    await _databaseService.UpdateTrainingFieldAsync(f);
            }
        }




        [RelayCommand]
        public void DeleteField(TrainingField field)
        {
            if (CustomFields.Contains(field))
                CustomFields.Remove(field);
        }






    }
}
