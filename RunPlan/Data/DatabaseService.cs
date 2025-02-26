using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RunPlan.Data
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            // Use the existing database file on your PC
            //string dbPath = @"C:\Users\petri\OneDrive\Desktop\Adv. C#\RunPlan\RunPlan\RunPlan\Data\database.db";
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "database.db");

            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<RunningActivity>().Wait();
        }

        // ✅ Insert a new activity
        public async Task InsertRunningActivity(string name, double distance, string time, string date)
        {
            var activity = new RunningActivity { Name = name, Distance = distance, Time = time, Date = date };
            await _database.InsertAsync(activity);
            Console.WriteLine("✅ Activity added to SQLite!");
        }

        // ✅ Retrieve all activities
        public async Task<List<RunningActivity>> GetAllActivitiesAsync()
        {
            return await _database.Table<RunningActivity>().ToListAsync();
        }
    }

    // ✅ Define the RunningActivity Model
    public class RunningActivity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Distance { get; set; }
        public string Time { get; set; } = "00:00:00";

        public string Date { get; set; } = "yyyy-MM-dd";
    }
}
