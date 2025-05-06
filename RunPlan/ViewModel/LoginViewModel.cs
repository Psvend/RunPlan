using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunPlan.Data;
using RunPlan.Model;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;

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
        [ObservableProperty]
         Color messageColor = Colors.Red;

        

       public LoginViewModel(DatabaseService db)
        {
            _db = db;
        }

        [RelayCommand]

        public async Task Login()
        {
            
            if(string.IsNullOrEmpty(Email) || string.IsNullOrWhiteSpace(Password))
            {
                Message = "Email and password cannot be empty";
                MessageColor = Colors.Red;
                return;
            }
            bool valid = await _db.ValidateUserAsync(Email, Password);
            if(valid)
            {
                Message = string.Empty;
                await Shell.Current.GoToAsync("//MainPage");
                Email = string.Empty;
                Password = string.Empty;
            }
            else
            {
                Message = "Invalid email or password";
                MessageColor = Colors.Red;
            }


        }
        [RelayCommand]
        public async Task Register()
        {
            if(!IsValidEmail(Email))
            {
                Message = "Invalid email format";
                MessageColor = Colors.Red;
                return;
            }

            bool success = await _db.RegisterUserAsync(Email, Password);
            if(success)
            {
                Message = "Registration successful";
                MessageColor = Colors.Green;

            }

            else
            {
                {
                    Message = "Email already exists";
                    MessageColor = Colors.Red;
                }
            }
        }
        [RelayCommand]
        public async Task Logout()
        {

            Email = string.Empty;
            Password = string.Empty;
            Message = string.Empty;

            await Shell.Current.GoToAsync("//LoginPage");
        }
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}