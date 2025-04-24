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
            // Trigger training loading right away
            _databaseService = databaseService;
            _ = LoadTrainingsAsync();
            Task.Run(async () => await LoadTrainingsAsync());

        }

        // Form inputs
        [ObservableProperty] private string name;
        [ObservableProperty] private string description;
        [ObservableProperty] private string time;
        [ObservableProperty] private string grade;

        [ObservableProperty] private ObservableCollection<Training> existingTrainings = new();

        [ObservableProperty] private bool isBusy;
        [ObservableProperty]
        private List<string> availableGrades = new()
        {
            "Super Easy",
            "Easy",
            "Medium",
            "Hard",
            "Extra Hard"
        };






        [RelayCommand]
        public async Task LoadTrainingsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                allTrainings = await _databaseService.GetAllTrainingsAsync();

                //await _databaseService.GetAllActivitiesAsync();

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






        //To make save function work
        [RelayCommand]
        public async Task SaveTraining()
        {
            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Time) ||
                string.IsNullOrWhiteSpace(Grade) ||
                string.IsNullOrWhiteSpace(Description))
            {
                await Shell.Current.DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            if (!int.TryParse(Time, out int timeInt))
            {
                await Shell.Current.DisplayAlert("Error", "Time must be a valid number.", "OK");
                return;
            }



            await _databaseService.InsertTrainingAsync(Name, Description, timeInt, Grade);

            // Clear inputs
            Name = Description = Time = Grade = string.Empty;

            await LoadTrainingsAsync();

            // Notify other components (if needed)
            WeakReferenceMessenger.Default.Send(new TrainingUpdatedMessage());
        }




    }
}
