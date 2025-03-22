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


        public void UpdateChartForLastThreeMonths()
        {
            var lastThreeMonthsData = new List<RunningDataModel>();
            DateTime today = DateTime.Now;
            CultureInfo culture = CultureInfo.InvariantCulture;

            Console.WriteLine("📊 Updating Chart for Last 3 Months...");

            // Step 1: Build the full range of Mondays (week starts)
            DateTime startDate = new DateTime(today.Year, today.Month, 1).AddMonths(-2);
            DateTime alignedStart = startDate.AddDays(-(int)(startDate.DayOfWeek == DayOfWeek.Sunday ? 6 : startDate.DayOfWeek - DayOfWeek.Monday));

            List<DateTime> allWeeks = new();
            for (DateTime date = alignedStart; date <= today; date = date.AddDays(7))
            {
                allWeeks.Add(date);
            }

            // Step 2: Map distances to week start
            Dictionary<DateTime, double> weeklySums = allWeeks.ToDictionary(w => w, w => 0.0);
            Dictionary<DateTime, string> weekMonthMap = new(); // Used to label correctly

            foreach (var activity in filteredActivities)
            {
                if (DateTime.TryParseExact(activity.Date, "yyyy-MM-dd", culture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    int delta = (7 + (int)parsedDate.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                    DateTime weekStart = parsedDate.AddDays(-delta);

                    if (weeklySums.ContainsKey(weekStart))
                    {
                        weeklySums[weekStart] += activity.Distance;
                        weekMonthMap[weekStart] = parsedDate.ToString("yyyy-MM"); // Store actual month from activity
                    }
                }
            }

            // Step 3: Convert to chart data
            foreach (var weekStart in allWeeks)
            {
                double distance = weeklySums[weekStart];
                string monthKey = weekMonthMap.ContainsKey(weekStart)
                    ? weekMonthMap[weekStart]
                    : weekStart.ToString("yyyy-MM"); // fallback

                lastThreeMonthsData.Add(new RunningDataModel
                {
                    WeekLabel = $"Week {ISOWeek.GetWeekOfYear(weekStart)}",
                    Distance = distance == 0 ? 0.05 : distance,
                    WeekNumber = ISOWeek.GetWeekOfYear(weekStart),
                    MonthKey = monthKey
                });
            }

            // Step 4: Sort and update chart
            lastThreeMonthsData = lastThreeMonthsData
                .OrderBy(d => d.MonthKey)
                .ThenBy(d => d.WeekNumber)
                .ToList();

            WeeklyRunningData.Clear();
            foreach (var item in lastThreeMonthsData)
            {
                WeeklyRunningData.Add(item);
            }

            ChartDrawable.Data = lastThreeMonthsData;
            OnPropertyChanged(nameof(ChartDrawable));

            Console.WriteLine($"✅ Chart updated with {lastThreeMonthsData.Count} weekly bars.");
        }







        /*

        //Controls the graph and how it sorts and shows the weeks/months

        public void UpdateChartForLastThreeMonths()
        {
            var lastThreeMonthsData = new List<RunningDataModel>();
            DateTime today = DateTime.Now;
            CultureInfo culture = CultureInfo.InvariantCulture;

            Console.WriteLine("📊 Updating Chart for Last 3 Months...");

            // Define date range (first day of 2 months ago to today)
            DateTime firstDay = new DateTime(today.Year, today.Month, 1).AddMonths(-2);
            DateTime lastDay = today;

            // 🔁 Get all Mondays (week starts)
            List<DateTime> allWeeks = new();
            DateTime currentWeek = firstDay;

            // Align to Monday
            while (currentWeek.DayOfWeek != DayOfWeek.Monday)
                currentWeek = currentWeek.AddDays(-1);

            while (currentWeek <= lastDay)
            {
                allWeeks.Add(currentWeek);
                currentWeek = currentWeek.AddDays(7);
            }

            // 🔢 Group distances by week start
            Dictionary<DateTime, double> weeklySums = new();
            foreach (var weekStart in allWeeks)
            {
                weeklySums[weekStart] = 0; // Default value
            }

            // ⏱ Assign each activity to the right weekly bucket
            foreach (var activity in filteredActivities)
            {
                if (DateTime.TryParseExact(activity.Date, "yyyy-MM-dd", culture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    // Find the Monday of that week
                    DateTime weekStart = parsedDate.AddDays(-(int)parsedDate.DayOfWeek + (parsedDate.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
                    if (weeklySums.ContainsKey(weekStart))
                    {
                        weeklySums[weekStart] += activity.Distance;
                    }
                }
            }




            // Build chart data
            foreach (var kvp in weeklySums.OrderBy(k => k.Key))
            {
                var weekStart = kvp.Key;
                var distance = kvp.Value == 0 ? 0.05 : kvp.Value;

                lastThreeMonthsData.Add(new RunningDataModel
                {
                    WeekLabel = $"Week {ISOWeek.GetWeekOfYear(weekStart)}",
                    Distance = distance,
                    WeekNumber = ISOWeek.GetWeekOfYear(weekStart),
                    MonthKey = weekStart.ToString("yyyy-MM")
                    //MonthKey = weekStart.ToString("yyyy-MM")
                });
            }
           


            // ✅ Sort by week date (already ordered above)
            WeeklyRunningData.Clear();
            foreach (var item in lastThreeMonthsData)
            {
                WeeklyRunningData.Add(item);
            }

            ChartDrawable.Data = lastThreeMonthsData;
            OnPropertyChanged(nameof(ChartDrawable));

            Console.WriteLine($"✅ Chart updated with {lastThreeMonthsData.Count} weekly bars.");
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




