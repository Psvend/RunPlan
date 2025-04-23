using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunPlan.Data;
using RunPlan.Model;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using RunPlan.Messages;



namespace RunPlan.ViewModel
{
    public partial class CreateTrainingViewModel : ObservableObject
    {
        private readonly DatabaseService _db;

        public CreateTrainingViewModel(DatabaseService databaseService)
        {
            _db = databaseService;
            //Training = new Training();
        }

        [ObservableProperty] private Training training;
        [ObservableProperty] private string name;
        [ObservableProperty] private string description;
        [ObservableProperty] private string time;
        [ObservableProperty] private string grade;





        [RelayCommand]
        public async Task SaveTraining()
        {
            if (int.TryParse(Time, out int timeInt) && int.TryParse(Grade, out int gradeInt))
            {
                await _db.InsertTrainingAsync(Name, Description, timeInt, gradeInt);

                // 🔁 Notify the app that a new training was added
                WeakReferenceMessenger.Default.Send(new TrainingUpdatedMessage());

                // 🔙 Go back to the TrainingList
                await Shell.Current.GoToAsync("//TrainingList");
            }
        }





    }
}

