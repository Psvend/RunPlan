
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using BCrypt.Net;


#if ANDROID
using Android.App;
using Android.OS;
#endif
#if IOS
using Foundation;
#endif




namespace RunPlan.Data
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private readonly string _dbPath;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public DatabaseService()
        {
            _dbPath = GetDatabasePath();
            Console.WriteLine($"📌 SQLite database path: {_dbPath}");
        }

        public async Task InitializeAsync()
        {
            _database = new SQLiteAsyncConnection(_dbPath);
            await _database.CreateTableAsync<RunningActivity>();
            await _database.CreateTableAsync<Training>();
            await _database.CreateTableAsync<User>();
            await EnsureDatabaseInitialized();
        }

        private string GetDatabasePath()
        {
            string dbFileName = "database.db";

#if WINDOWS
            string projectFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            if (!Directory.Exists(projectFolder))
                Directory.CreateDirectory(projectFolder);
            return Path.Combine(projectFolder, dbFileName);
#else
            string folderPath;

#if ANDROID
            folderPath = Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath;
            dbFileName = "database_android.db";
            return Path.Combine(folderPath, dbFileName);
#else
            folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif
            return Path.Combine(folderPath, dbFileName);
#endif
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

        public async Task InsertRunningActivity(string name, double distance, string time, string date)
        {
            var activity = new RunningActivity { Name = name, Distance = distance, Time = time, Date = date };
            await _lock.WaitAsync();
            try
            {
                await _database.InsertAsync(activity);
                Console.WriteLine("✅ Activity added to SQLite!");
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<List<RunningActivity>> GetAllActivitiesAsync()
        {
            await _lock.WaitAsync();
            try
            {
                return await _database.Table<RunningActivity>().ToListAsync();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task DeleteActivity(int id)
        {
            await _lock.WaitAsync();
            try
            {
                await _database.DeleteAsync<RunningActivity>(id);
                Console.WriteLine($"🗑️ Deleted activity with ID: {id}");
            }
            finally
            {
                _lock.Release();
            }
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

        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = await _database.Table<User>().FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                Console.WriteLine("Password hash is empty");
                return false;
            }
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        public string GetDbPath() => _dbPath;

        public async Task UpdateActivityAsync(RunningActivity activity)
        {
            if (activity != null)
            {
                await _lock.WaitAsync();
                try
                {
                    await _database.UpdateAsync(activity);
                    Console.WriteLine($"✅ Activity updated: {activity.Name}, ID: {activity.Id}");
                }
                finally
                {
                    _lock.Release();
                }
            }
        }

        public async Task<RunningActivity> GetActivityByIdAsync(int id)
        {
            return await _database.Table<RunningActivity>().FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task InsertTrainingAsync(string name, string description, int time, int grade)
        {
            var training = new Training { Name = name, Description = description, Time = time, Grade = grade };
            await _lock.WaitAsync();
            try
            {
                await _database.InsertAsync(training);
                Console.WriteLine($"✅ Training added: {training.Name}");
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<List<Training>> GetAllTrainingsAsync()
        {
            await _lock.WaitAsync();
            try
            {
                return await _database.Table<Training>().ToListAsync();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task DeleteTrainingAsync(int id)
        {
            await _lock.WaitAsync();
            try
            {
                await _database.DeleteAsync<Training>(id);
                Console.WriteLine($"🗑️ Training deleted: ID {id}");
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task UpdateTrainingAsync(Training training)
        {
            if (training != null)
            {
                await _lock.WaitAsync();
                try
                {
                    await _database.UpdateAsync(training);
                    Console.WriteLine($"✅ Training updated: {training.Name}, ID: {training.Id}");
                }
                finally
                {
                    _lock.Release();
                }
            }
        }
    }
}
