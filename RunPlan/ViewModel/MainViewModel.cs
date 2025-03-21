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
        public readonly DatabaseService _dbService;
        public ObservableCollection<RunningDataModel> WeeklyRunningData { get; set; }
        public BarChartDrawable ChartDrawable { get; set; }
        public Dictionary<string, List<RunningDataModel>> MonthlyData { get; private set; }
        private List<RunningActivity> filteredActivities = new List<RunningActivity>(); // 🔥 Global List



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

            //Task.Run(LoadActivities);
            _ = LoadActivities();
        }


        public async Task LoadActivities()
        {
            Console.WriteLine("📥 Fetching activities from database...");

            var activities = await _dbService.GetAllActivitiesAsync();
            if (activities == null || activities.Count == 0)
            {
                Console.WriteLine("⚠ No activities found in the database!");
                return; // ⛔ Don't proceed if no data
            }

            DateTime today = DateTime.Now;
            DateTime firstDayOfThisMonth = new DateTime(today.Year, today.Month, 1);
            DateTime firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);
            DateTime firstDayOfTwoMonthsAgo = firstDayOfThisMonth.AddMonths(-2);

            filteredActivities = activities
                .Where(a => DateTime.TryParseExact(a.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                                   DateTimeStyles.None, out DateTime parsedDate)
                            && parsedDate >= firstDayOfTwoMonthsAgo)
                .ToList();

            Console.WriteLine($"✅ Loaded {filteredActivities.Count} activities.");
            foreach (var act in filteredActivities)
            {
                Console.WriteLine($"📌 Activity: {act.Name} | {act.Distance} km | {act.Date}");
            }

            MonthlyData.Clear();
            UpdateChartForLastThreeMonths();
        }




        /*
        public async Task LoadActivities()
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
            CultureInfo culture = CultureInfo.InvariantCulture;
            Calendar calendar = culture.Calendar;

            // ✅ Step 1: Ensure all weeks exist in the last 3 months (Even if Empty)
            for (int i = 0; i < 3; i++)
            {
                string monthKey = today.AddMonths(-i).ToString("yyyy-MM");
                DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                if (!MonthlyData.ContainsKey(monthKey))
                    MonthlyData[monthKey] = new List<RunningDataModel>();

                Dictionary<int, double> weeklyDistances = new Dictionary<int, double>();

                for (DateTime weekStart = firstDayOfMonth; weekStart <= lastDayOfMonth; weekStart = weekStart.AddDays(7))
                {
                    int weekNumber = calendar.GetWeekOfYear(weekStart, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    weeklyDistances[weekNumber] = 0;
                }

                // ✅ Step 2: Fill in actual distances
                foreach (var activity in filteredActivities)
                {
                    if (DateTime.TryParseExact(activity.Date, "yyyy-MM-dd", culture, DateTimeStyles.None, out DateTime parsedDate))
                    {
                        int weekNumber = calendar.GetWeekOfYear(parsedDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                        if (weeklyDistances.ContainsKey(weekNumber))
                            weeklyDistances[weekNumber] += activity.Distance;
                    }
                }

                // ✅ Step 3: Convert weeks into chart data
                var sortedWeeks = weeklyDistances
                    .OrderBy(w => w.Key)
                    .Select(kvp => new RunningDataModel
                    {
                        WeekLabel = $"Week {kvp.Key}",
                        Distance = kvp.Value
                    })
                    .ToList();

                MonthlyData[monthKey] = sortedWeeks;
            }

            UpdateChartForLastThreeMonths();
        }
        */






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


        //Handles the graph logic
        public void UpdateChartForLastThreeMonths()
        {
            var lastThreeMonthsData = new List<RunningDataModel>();
            DateTime today = DateTime.Now;
            CultureInfo culture = CultureInfo.InvariantCulture;

            // ✅ Get last 3 months (Newest Month Last)
            var monthKeys = Enumerable.Range(0, 3)
                .Select(i => today.AddMonths(-i).ToString("yyyy-MM"))
                .OrderBy(m => m) // ✅ Ensure months are ordered correctly
                .ToList();

            Console.WriteLine($"📅 Processing months: {string.Join(", ", monthKeys)}");

            foreach (var monthKey in monthKeys)
            {
                if (!MonthlyData.ContainsKey(monthKey))
                {
                    MonthlyData[monthKey] = new List<RunningDataModel>();
                }

                Dictionary<int, double> weeklyDistances = new Dictionary<int, double>();

                // ✅ Ensure activities exist for this month
                var activitiesForMonth = filteredActivities.Where(a => a.Date.StartsWith(monthKey)).ToList();
                if (activitiesForMonth.Count == 0)
                {
                    Console.WriteLine($"⚠ No activities found for {monthKey}");
                }

                // ✅ Process activities and map them to weeks
                foreach (var activity in activitiesForMonth)
                {
                    if (DateTime.TryParseExact(activity.Date, "yyyy-MM-dd", culture, DateTimeStyles.None, out DateTime parsedDate))
                    {
                        int weekNumber = ISOWeek.GetWeekOfYear(parsedDate);

                        if (!weeklyDistances.ContainsKey(weekNumber))
                        {
                            weeklyDistances[weekNumber] = 0; // ✅ Initialize empty weeks
                        }

                        weeklyDistances[weekNumber] += activity.Distance;
                    }
                }

                // ✅ Convert to RunningDataModel for the graph
                var sortedWeeks = weeklyDistances
                    .OrderBy(w => w.Key)
                    .Select(kvp => new RunningDataModel
                    {
                        WeekLabel = $"Week {kvp.Key}", // ✅ Keep a readable label
                        WeekNumber = kvp.Key, // ✅ Store numerical week number
                        MonthKey = monthKey, // ✅ Store the month for sorting
                        Distance = kvp.Value == 0 ? 0.05 : kvp.Value // ✅ Ensure empty weeks show up
                    })
                    .ToList();

                lastThreeMonthsData.AddRange(sortedWeeks);
            }

            // ✅ Final Sorting: Newest month on right, weeks sorted properly
            lastThreeMonthsData = lastThreeMonthsData
                .OrderBy(d => monthKeys.IndexOf(d.MonthKey)) // ✅ Sort months first
                .ThenBy(d => d.WeekNumber) // ✅ Then sort by week number
                .ToList();

            // ✅ Debugging: Print data
            Console.WriteLine($"✅ Final Sorted Data: {string.Join(", ", lastThreeMonthsData.Select(d => $"{d.MonthKey}-W{d.WeekNumber}: {d.Distance}km"))}");

            // ✅ Update UI
            WeeklyRunningData.Clear();
            foreach (var item in lastThreeMonthsData)
            {
                WeeklyRunningData.Add(item);
            }

            ChartDrawable.Data = lastThreeMonthsData;
            OnPropertyChanged(nameof(ChartDrawable));

            Console.WriteLine($"✅ Chart updated with {lastThreeMonthsData.Count} bars.");
        }












        /*
        public void UpdateChartForLastThreeMonths()
        {
            var lastThreeMonthsData = new List<RunningDataModel>();
            DateTime today = DateTime.Now;
            CultureInfo culture = CultureInfo.InvariantCulture;
            Calendar calendar = culture.Calendar;

            Console.WriteLine("📊 Updating Chart for Last 3 Months...");

            for (int i = 2; i >= 0; i--) // Process last 3 months (latest month last)
            {
                string monthKey = today.AddMonths(-i).ToString("yyyy-MM");
                DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                Console.WriteLine($"📌 Processing Month: {monthKey}");

                if (!MonthlyData.ContainsKey(monthKey))
                {
                    Console.WriteLine($"⚠ No data for {monthKey}, initializing empty list.");
                    MonthlyData[monthKey] = new List<RunningDataModel>();
                }

                Dictionary<int, double> weeklyDistances = new Dictionary<int, double>();

                // ✅ Ensure all weeks in the month exist
                for (DateTime weekStart = firstDayOfMonth; weekStart <= lastDayOfMonth; weekStart = weekStart.AddDays(7))
                {
                    int weekNumber = ISOWeek.GetWeekOfYear(weekStart);
                    if (!weeklyDistances.ContainsKey(weekNumber))
                        weeklyDistances[weekNumber] = 0; // Default to 0 km
                }

                Console.WriteLine($"📊 Weeks Initialized for {monthKey}: {string.Join(", ", weeklyDistances.Keys)}");

                // ✅ Map real activity distances to weeks
                foreach (var activity in filteredActivities) // 🔥 FIX: Use RunningActivity instead of RunningDataModel
                {
                    if (DateTime.TryParseExact(activity.Date, "yyyy-MM-dd", culture, DateTimeStyles.None, out DateTime parsedDate))
                    {
                        int weekNumber = ISOWeek.GetWeekOfYear(parsedDate);
                        if (weeklyDistances.ContainsKey(weekNumber))
                        {
                            weeklyDistances[weekNumber] += activity.Distance;
                            Console.WriteLine($"✅ {activity.Name} ({activity.Date}) added to Week {weekNumber} -> {weeklyDistances[weekNumber]} km");
                        }
                    }
                }

                // ✅ Convert weekly data into chart-friendly format
                var sortedWeeks = weeklyDistances
                    .Where(kvp => kvp.Key <= ISOWeek.GetWeekOfYear(today)) // ✅ Only include up to current week
                    .OrderBy(kvp => kvp.Key) // ✅ Sort weeks correctly
                    .Select(kvp => new RunningDataModel
                    {
                        WeekLabel = $"Week {kvp.Key}",
                        Distance = kvp.Value == 0 ? 0.05 : kvp.Value // ✅ Small bar for empty weeks
                    })
                    .ToList();

                lastThreeMonthsData.AddRange(sortedWeeks);
            }

            // ✅ Fix Sorting: Ensure latest month is on the right
            lastThreeMonthsData = lastThreeMonthsData
                .OrderByDescending(d => d.WeekLabel) // Sort by week number
                .ToList();

            // ✅ Print final data before updating chart
            Console.WriteLine($"📊 Final Data for Chart: {string.Join(", ", lastThreeMonthsData.Select(w => $"{w.WeekLabel}: {w.Distance} km"))}");

            // ✅ Update UI Data
            WeeklyRunningData.Clear();
            foreach (var item in lastThreeMonthsData)
            {
                WeeklyRunningData.Add(item);
            }

            ChartDrawable.Data = lastThreeMonthsData;
            OnPropertyChanged(nameof(ChartDrawable));

            Console.WriteLine($"✅ Chart updated with {lastThreeMonthsData.Count} bars.");
        }

        */




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