using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunPlan.Data;
using RunPlan.Model;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using RunPlan.Messages; 


namespace RunPlan.ViewModel
{
    public partial class TrainingListViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        public TrainingListViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Trainings = new ObservableCollection<Training>();
            AllTrainings = new List<Training>();
            
            // Listen for update messages
            WeakReferenceMessenger.Default.Register<TrainingUpdatedMessage>(this, async (r, m) =>
            {
                await LoadTrainingsAsync();
            });
        }

        [ObservableProperty]
        private string searchQuery;

        public ObservableCollection<Training> Trainings { get; set; }
        private List<Training> AllTrainings { get; set; }



        public async Task LoadTrainingsAsync()
        {
            var list = await _databaseService.GetAllTrainingsAsync();
            AllTrainings = list;
            Trainings.Clear();

            foreach (var item in list)
                Trainings.Add(item);
        }



        public void FilterTrainingsBySearch()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                Trainings.Clear();
                foreach (var item in AllTrainings)
                    Trainings.Add(item);
            }
            else
            {
                var filtered = AllTrainings
                    .Where(t => t.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

                Trainings.Clear();
                foreach (var item in filtered)
                    Trainings.Add(item);
            }
        }
    }
}

