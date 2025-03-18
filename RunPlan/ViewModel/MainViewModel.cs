using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RunPlan.Model;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input; 



namespace RunPlan.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        public ObservableCollection<RunningDataModel> WeeklyRunningData { get; set; }
        public BarChartDrawable ChartDrawable { get; set; }
        public Dictionary<string, List<RunningDataModel>> MonthlyData { get; private set; }

        private string _selectedMonth;
        public string SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (_selectedMonth != value)
                {
                    _selectedMonth = value;
                    OnPropertyChanged();
                    UpdateChartForMonth(_selectedMonth);
                }
            }
        }

        public ObservableCollection<string> AvailableMonths { get; set; }

        // Commands for UI Binding
        public ICommand AddActivityCommand { get; }
        public ICommand DeleteActivityCommand { get; }

        public MainViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            WeeklyRunningData = new ObservableCollection<RunningDataModel>();
            ChartDrawable = new BarChartDrawable { Data = new List<RunningDataModel>() };
            MonthlyData = new Dictionary<string, List<RunningDataModel>>();
            AvailableMonths = new ObservableCollection<string>();

            // Initialize commands
            AddActivityCommand = new AsyncRelayCommand(AddActivity);
            DeleteActivityCommand = new AsyncRelayCommand<RunningActivity>(DeleteActivity);

            Task.Run(LoadActivities);
        }

        private async Task LoadActivities()
        {
            var activities = await _dbService.GetAllActivitiesAsync();

            DateTime today = DateTime.Now;
            DateTime firstDayOfThisMonth = new DateTime(today.Year, today.Month, 1);
            DateTime firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);
            DateTime firstDayOfTwoMonthsAgo = firstDayOfThisMonth.AddMonths(-2);

            var filteredActivities = activities
                .Where(a => DateTime.TryParseExact(a.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                                   DateTimeStyles.None, out DateTime parsedDate)
                            && parsedDate >= firstDayOfTwoMonthsAgo)
                .ToList();

            MonthlyData.Clear();
            AvailableMonths.Clear();
            CultureInfo culture = CultureInfo.InvariantCulture;
            Calendar calendar = culture.Calendar;

            foreach (var activity in filteredActivities)
            {
                if (DateTime.TryParseExact(activity.Date, "yyyy-MM-dd", culture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    string monthKey = parsedDate.ToString("yyyy-MM");
                    int weekNumber = calendar.GetWeekOfYear(parsedDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                    if (!MonthlyData.ContainsKey(monthKey))
                        MonthlyData[monthKey] = new List<RunningDataModel>();

                    var weekData = MonthlyData[monthKey].FirstOrDefault(w => w.WeekLabel == $"Week {weekNumber}");
                    if (weekData == null)
                    {
                        MonthlyData[monthKey].Add(new RunningDataModel
                        {
                            WeekLabel = $"Week {weekNumber}",
                            Distance = activity.Distance
                        });
                    }
                    else
                    {
                        weekData.Distance += activity.Distance;
                    }

                    if (!AvailableMonths.Contains(monthKey))
                        AvailableMonths.Add(monthKey);
                }
            }

            AvailableMonths.Insert(0, "Last 3 Months");
            SelectedMonth = today.ToString("yyyy-MM"); // Default selection
        }

        public void UpdateChartForMonth(string selectedMonth)
        {
            if (selectedMonth == "Last 3 Months")
            {
                var lastThreeMonths = new List<RunningDataModel>();
                DateTime today = DateTime.Now;

                for (int i = 0; i < 3; i++)
                {
                    string monthKey = today.AddMonths(-i).ToString("yyyy-MM");
                    if (MonthlyData.ContainsKey(monthKey))
                    {
                        lastThreeMonths.AddRange(MonthlyData[monthKey]);
                    }
                }

                WeeklyRunningData.Clear();
                foreach (var item in lastThreeMonths.OrderBy(d => d.WeekLabel))
                {
                    WeeklyRunningData.Add(item);
                }

                ChartDrawable.Data = lastThreeMonths;
            }
            else if (MonthlyData.ContainsKey(selectedMonth))
            {
                var selectedData = MonthlyData[selectedMonth].OrderBy(d => d.WeekLabel).ToList();

                WeeklyRunningData.Clear();
                foreach (var item in selectedData)
                {
                    WeeklyRunningData.Add(item);
                }

                ChartDrawable.Data = selectedData;
            }

            ChartDrawable.Data = WeeklyRunningData.ToList();
            OnPropertyChanged(nameof(ChartDrawable));
        }

        // ✅ Handle adding a new activity
        private async Task AddActivity()
        {
            Console.WriteLine("Add Activity Button Clicked!");

            if (string.IsNullOrEmpty(ActivityName) || string.IsNullOrEmpty(DistanceText) || string.IsNullOrEmpty(Time) || string.IsNullOrEmpty(Date))
            {
                Console.WriteLine("❌ Error: Please fill in all fields.");
                return;
            }

            if (!double.TryParse(DistanceText, out double distance))
            {
                Console.WriteLine("❌ Error: Distance must be a valid number.");
                return;
            }

            await _dbService.InsertRunningActivity(ActivityName, distance, Time, Date);

            Console.WriteLine("✅ New Activity Added! Refreshing Chart...");
            await LoadActivities(); // Refresh UI
        }

        // ✅ Handle deleting an activity
        private async Task DeleteActivity(RunningActivity activity)
        {
            if (activity == null) return;

            Console.WriteLine($"❌ Deleting activity: {activity.Name}");

            await _dbService.DeleteActivity(activity.Id);
            await LoadActivities(); // Refresh UI after deletion
        }

        // ✅ Calculate Running Pace (min/km)
        public static string CalculatePace(string time, double distance)
        {
            try
            {
                string[] parts = time.Split(':'); // Expecting format "hh:mm:ss"
                if (parts.Length != 3) return "N/A";

                int hours = int.Parse(parts[0]);
                int minutes = int.Parse(parts[1]);
                int seconds = int.Parse(parts[2]);

                double totalMinutes = hours * 60 + minutes + (seconds / 60.0);
                double pace = totalMinutes / distance;

                int paceMinutes = (int)pace;
                int paceSeconds = (int)((pace - paceMinutes) * 60);

                return $"{paceMinutes}:{paceSeconds:00} min/km";
            }
            catch
            {
                return "N/A"; // Return default if calculation fails
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Properties to bind to UI (Entry fields)
        public string ActivityName { get; set; }
        public string DistanceText { get; set; }
        public string Time { get; set; }
        public string Date { get; set; }
    }
}










/*
namespace RunPlan.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        public ObservableCollection<RunningDataModel> WeeklyRunningData { get; set; }
        public BarChartDrawable ChartDrawable { get; set; }
        public Dictionary<string, List<RunningDataModel>> MonthlyData { get; private set; }

        private string _selectedMonth;
        public string SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (_selectedMonth != value)
                {
                    _selectedMonth = value;
                    OnPropertyChanged();
                    UpdateChartForMonth(_selectedMonth);
                }
            }
        }

        public ObservableCollection<string> AvailableMonths { get; set; }

        public MainViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            WeeklyRunningData = new ObservableCollection<RunningDataModel>();
            ChartDrawable = new BarChartDrawable { Data = new List<RunningDataModel>() };
            MonthlyData = new Dictionary<string, List<RunningDataModel>>();
            AvailableMonths = new ObservableCollection<string>();

            Task.Run(LoadActivities);
        }

        private async Task LoadActivities()
        {
            var activities = await _dbService.GetAllActivitiesAsync();

            DateTime today = DateTime.Now;
            DateTime firstDayOfThisMonth = new DateTime(today.Year, today.Month, 1);
            DateTime firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);
            DateTime firstDayOfTwoMonthsAgo = firstDayOfThisMonth.AddMonths(-2);

            var filteredActivities = activities
                .Where(a => DateTime.TryParseExact(a.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                                   DateTimeStyles.None, out DateTime parsedDate)
                            && parsedDate >= firstDayOfTwoMonthsAgo)
                .ToList();

            MonthlyData.Clear();
            AvailableMonths.Clear();
            CultureInfo culture = CultureInfo.InvariantCulture;
            Calendar calendar = culture.Calendar;

            foreach (var activity in filteredActivities)
            {
                if (DateTime.TryParseExact(activity.Date, "yyyy-MM-dd", culture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    string monthKey = parsedDate.ToString("yyyy-MM");
                    int weekNumber = calendar.GetWeekOfYear(parsedDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                    if (!MonthlyData.ContainsKey(monthKey))
                        MonthlyData[monthKey] = new List<RunningDataModel>();

                    var weekData = MonthlyData[monthKey].FirstOrDefault(w => w.WeekLabel == $"Week {weekNumber}");
                    if (weekData == null)
                    {
                        MonthlyData[monthKey].Add(new RunningDataModel
                        {
                            WeekLabel = $"Week {weekNumber}",
                            Distance = activity.Distance
                        });
                    }
                    else
                    {
                        weekData.Distance += activity.Distance;
                    }

                    if (!AvailableMonths.Contains(monthKey))
                        AvailableMonths.Add(monthKey);
                }
            }

            AvailableMonths.Insert(0, "Last 3 Months");

            // Set default selection to "This Month"
            SelectedMonth = today.ToString("yyyy-MM");
        }

        public void UpdateChartForMonth(string selectedMonth)
        {
            if (selectedMonth == "Last 3 Months")
            {
                var lastThreeMonths = new List<RunningDataModel>();
                DateTime today = DateTime.Now;

                for (int i = 0; i < 3; i++)
                {
                    string monthKey = today.AddMonths(-i).ToString("yyyy-MM");
                    if (MonthlyData.ContainsKey(monthKey))
                    {
                        lastThreeMonths.AddRange(MonthlyData[monthKey]);
                    }
                }

                WeeklyRunningData.Clear();
                foreach (var item in lastThreeMonths.OrderBy(d => d.WeekLabel))
                {
                    WeeklyRunningData.Add(item);
                }

                ChartDrawable.Data = lastThreeMonths;
            }
            else if (MonthlyData.ContainsKey(selectedMonth))
            {
                var selectedData = MonthlyData[selectedMonth].OrderBy(d => d.WeekLabel).ToList();

                WeeklyRunningData.Clear();
                foreach (var item in selectedData)
                {
                    WeeklyRunningData.Add(item);
                }

                ChartDrawable.Data = selectedData;
            }

            ChartDrawable.Data = WeeklyRunningData.ToList();
            OnPropertyChanged(nameof(ChartDrawable));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }









    }
}
*/