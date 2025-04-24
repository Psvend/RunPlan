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
            Task.Run(LoadTrainingsAsync);

        }

        // Form inputs
        [ObservableProperty] private string name;
        [ObservableProperty] private string description;
        [ObservableProperty] private string time;
        [ObservableProperty] private string grade;

        [ObservableProperty] private ObservableCollection<Training> existingTrainings = new();

        [ObservableProperty] private bool isBusy;






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
    }
}
