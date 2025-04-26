using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunPlan.Data;
using RunPlan.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace RunPlan.ViewModel
{
    public partial class TrainingListViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        // Use partial properties instead of [ObservableProperty] for AOT compatibility
        private ObservableCollection<Training> _trainings = new();
        public ObservableCollection<Training> Trainings
        {
            get => _trainings;
            set => SetProperty(ref _trainings, value);
        }

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    OnSearchQueryChanged(value); // Call the partial method
                }
            }
        }

        private bool _isEmpty = true;
        public bool IsEmpty
        {
            get => _isEmpty;
            set => SetProperty(ref _isEmpty, value);
        }

        private List<Training> allTrainings = new();

        public TrainingListViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [RelayCommand]
        public async Task LoadTrainingsAsync()
        {
            try
            {
                var list = await _databaseService.GetAllTrainingsAsync() ?? new List<Training>();
                allTrainings = list;
                Trainings.Clear();
                IsEmpty = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading trainings: {ex.Message}");
            }
        }

        // Implement the partial method
        partial void OnSearchQueryChanged(string value)
        {
            FilterTrainings();
        }

        private void FilterTrainings()
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var filtered = allTrainings.Where(t => t.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
                Trainings = new ObservableCollection<Training>(filtered);
                IsEmpty = Trainings.Count == 0;
            }
            else
            {
                Trainings.Clear();
                IsEmpty = true;
            }
        }
    }
}
