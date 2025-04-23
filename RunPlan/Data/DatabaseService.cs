
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using BCrypt.Net; 

#if ANDROID
using Android.App;  // ✅ Ensures correct Application class is used
using Android.OS;
#endif
#if IOS
using Foundation;
#endif

namespace RunPlan.Data
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;
        private readonly string _dbPath;

        public DatabaseService()
        {
            _dbPath = GetDatabasePath();

            Console.WriteLine($"📌 SQLite database path: {_dbPath}");

            try
            {
                _database = new SQLiteAsyncConnection(_dbPath);
                _database.CreateTableAsync<RunningActivity>().Wait();
                _database.CreateTableAsync<User>().Wait();

                // ✅ Ensure database exists by adding a test record if empty
                EnsureDatabaseInitialized().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database initialization failed: {ex.Message}");
            }
        }


        private string GetDatabasePath()
        {
            string dbFileName = "database.db"; // Default name

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // ✅ Windows: Store in a shared "Data" folder inside the project
                string projectFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data");

                // ✅ Ensure directory exists
                if (!Directory.Exists(projectFolder))
                {
                    Directory.CreateDirectory(projectFolder);
                }

                return Path.Combine(projectFolder, dbFileName);
            }

            // ✅ Android & iOS: Store in app’s internal storage
            string folderPath;

#if ANDROID
            folderPath = Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath;
            dbFileName = "database_android.db";  // ✅ Android gets a separate database
            string androidDbPath = Path.Combine(folderPath, dbFileName);

            return androidDbPath;
#else
            folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); // Default fallback
#endif
            return Path.Combine(folderPath, dbFileName);
        }

        private async Task EnsureDatabaseInitialized()
        {
            var count = await _database.Table<RunningActivity>().CountAsync();
            if (count == 0)
            {
                var testRecord = new RunningActivity
                {
                    Name = "Test Run",
                    Distance = 5.0,
                    Time = "00:30:00",
                    Date = "2025-03-05"
                };
                await _database.InsertAsync(testRecord);
                Console.WriteLine("✅ Test record added to ensure DB is accessible.");
            }
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

        // ✅ Delete an activity
        public async Task DeleteActivity(int id)
        {
            await _database.DeleteAsync<RunningActivity>(id);
            Console.WriteLine($"🗑️ Deleted activity with ID: {id}");
        }

        public async Task<bool> RegisterUserAsync(string email, string password)
        {
            var existing = await _database.Table<User>().FirstOrDefaultAsync(u => u.Email == email);
            if (existing != null) return false;
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Email = email, PasswordHash = hashedPassword };
            await _database.InsertAsync(user);
            Console.WriteLine("User registered with hash password");
            return true;
        }
        public async Task<bool>ValidateUserAsync(string email, string password)
        {
            var user = await _database.Table<User>().FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;
            if(string.IsNullOrEmpty(user.PasswordHash))
            { Console.WriteLine("Password hash is empty"); return false; }
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        }
        // ✅ Get database path (for debugging)
        public string GetDbPath()
        {
            return _dbPath;
        }



        //To help edit page
        public async Task UpdateActivityAsync(RunningActivity activity)
        {
            if (activity != null)
            {
                await _database.UpdateAsync(activity);
                Console.WriteLine($"✅ Activity updated: {activity.Name}, ID: {activity.Id}");
            }
        }


        public async Task<RunningActivity> GetActivityByIdAsync(int id)
        {
            return await _database.Table<RunningActivity>()
                                  .FirstOrDefaultAsync(a => a.Id == id);
        }



    }
}



        /*
        // ✅ Define the RunningActivity Model
        public class RunningActivity
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public double Distance { get; set; }
            public string Time { get; set; } = "00:00:00";
            public string Date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
            public string Grade { get; set; } = "N/A";
            public string Description { get; set; } = string.Empty; // Optional field

        }
    }
        */

