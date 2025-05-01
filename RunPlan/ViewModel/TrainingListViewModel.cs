using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunPlan.Data;
using RunPlan.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using Google.Protobuf.WellKnownTypes;
using System.Globalization;


namespace RunPlan.ViewModel
{
    public partial class TrainingListViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private List<Training> allTrainings = new();

        public ObservableCollection<Training> Trainings { get; } = new();

        [ObservableProperty] private string searchQuery = string.Empty;
        [ObservableProperty] private List<string> availableGrades = new();
        [ObservableProperty] private string selectedGrade = "All";

        [ObservableProperty] private bool isEmpty = true;

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

                // Get available grades from trainings (distinct)
                var grades = allTrainings
                    .Select(t => t.Grade)
                    .Where(g => !string.IsNullOrEmpty(g))
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();

                grades.Insert(0, "All"); // Add "All" at the top
                AvailableGrades = grades;

                SelectedGrade = "All";

                ApplyFilter(allTrainings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading trainings: {ex.Message}");
            }
        }

        partial void OnSelectedGradeChanged(string value)
        {
            ApplyFilter(allTrainings);
        }

        partial void OnSearchQueryChanged(string value)
        {
            ApplyFilter(allTrainings);
        }

        private void ApplyFilter(IEnumerable<Training> trainings)
        {
            var filtered = trainings;

            if (SelectedGrade != "All")
            {
                filtered = filtered.Where(t => t.Grade == SelectedGrade);
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                filtered = filtered.Where(t => t.Name != null &&
                                               t.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }

            Trainings.Clear();
            foreach (var t in filtered)
                Trainings.Add(t);

            IsEmpty = Trainings.Count == 0;
        }

        public void DeleteTraining(Training training)
        {
            if (training == null) return;

            Trainings.Remove(training);
            allTrainings.Remove(training);
            IsEmpty = Trainings.Count == 0;
        }
    }
}
