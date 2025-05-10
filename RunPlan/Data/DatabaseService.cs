
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
            CopySeedDatabaseIfNeeded().Wait();
            _database = new SQLiteAsyncConnection(_dbPath);
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _database.CreateTableAsync<RunningActivity>();
                await _database.CreateTableAsync<Training>();
                //await _database.CreateTableAsync<User>();
                await _database.CreateTableAsync<TrainingField>();
                await EnsureDatabaseInitialized();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database initialization failed: {ex.Message}");
            }
        }

        private async Task CopySeedDatabaseIfNeeded()
        {
            if (!File.Exists(_dbPath))
            {
                try
                {
                    using var stream = await FileSystem.OpenAppPackageFileAsync("database.db");
                    using var dest = File.Create(_dbPath);
                    await stream.CopyToAsync(dest);
                    Console.WriteLine("✅ Copied bundled DB to app storage.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to copy DB: {ex.Message}");
                }
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
            folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            dbFileName = "database.db";  // ✅ Android gets a separate database
            string androidDbPath = Path.Combine(folderPath, dbFileName);

            return androidDbPath;
#else
            folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData); // Default fallback
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
        public async Task InsertRunningActivity(string name, double distance, string time, string date, string grade, string describtion)
        {
            var activity = new RunningActivity { Name = name, Distance = distance, Time = time, Date = date, Grade = grade, Description = describtion };
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



        // Insert a new training session
        public async Task InsertTrainingAsync(string name, string description, int time, string grade, int distance)
        {
            var training = new Training
            {
                Name = name,
                Description = description,
                Time = time,
                Grade = grade,
                Distance= distance


            };

            await _database.InsertAsync(training);
            Console.WriteLine($"✅ Training added: {training.Name}");
        }

        // Retrieve all trainings
        public async Task<List<Training>> GetAllTrainingsAsync()
        {
            return await _database.Table<Training>().ToListAsync();
        }

        // Delete a training
        public async Task DeleteTrainingAsync(int id)
        {
            await _database.DeleteAsync<Training>(id);
            Console.WriteLine($"🗑️ Training deleted: ID {id}");
        }

        // Update training
        public async Task UpdateTrainingAsync(Training training)
        {
            if (training != null)
            {
                await _database.UpdateAsync(training);
                Console.WriteLine($"✅ Training updated: {training.Name}, ID: {training.Id}");
            }
        }



        //TRAINING FIELD FEATURE
        // to insert a new field:
        public async Task InsertTrainingFieldAsync(TrainingField field)
        {
            await _database.InsertAsync(field);
        }

        // to load all fields for one training, ordered by SortOrder:
        public async Task<List<TrainingField>> GetFieldsForTrainingAsync(int trainingId)
        {
            return await _database.Table<TrainingField>()
                                  .Where(f => f.TrainingId == trainingId)
                                  .OrderBy(f => f.SortOrder)
                                  .ToListAsync();
        }

        // to update or delete, same pattern...
        public async Task UpdateTrainingFieldAsync(TrainingField field)
            => await _database.UpdateAsync(field);

        public async Task DeleteTrainingFieldAsync(int fieldId)
            => await _database.DeleteAsync<TrainingField>(fieldId);



        //To link training and trainingField together
        public async Task<int> InsertTrainingWithFieldsAsync(Training training, List<TrainingField> fields)
        {
            int trainingId = 0;

            await _database.RunInTransactionAsync(conn =>
            {
                conn.Insert(training);
                trainingId = training.Id;

                foreach (var field in fields)
                {
                    field.TrainingId = trainingId;
                    conn.Insert(field);
                }
            });

            return trainingId;
        }




    }
}




