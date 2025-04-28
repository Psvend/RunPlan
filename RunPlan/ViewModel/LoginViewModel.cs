using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunPlan.Data;
using RunPlan.Model;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace RunPlan.ViewModel
{
    public partial class LoginViewModel: ObservableObject
    {
        private readonly DatabaseService _db;
        [ObservableProperty]
        string email;
        [ObservableProperty]
        string password;
        [ObservableProperty]
        string message;

       public LoginViewModel(DatabaseService db)
        {
            _db = db;
        }

        [RelayCommand]

        public async Task Login()
        {
            bool valid = await _db.ValidateUserAsync(Email, Password);
            if(valid)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                Message = "Invalid email or password";
               
            }


        }
        [RelayCommand]
        public async Task Register()
        {
            bool success = await _db.RegisterUserAsync(Email, Password);
            Message = success ? "Registration successful" : "Registration failed";
        }
        [RelayCommand]
        public async Task Logout()
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}