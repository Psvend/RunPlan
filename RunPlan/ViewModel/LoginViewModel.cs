using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunPlan.Data;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using RunPlan.Model;
namespace RunPlan.ViewModel;

public partial class LoginViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly MainPage _mainPage;

    [ObservableProperty] string email;
    [ObservableProperty] string password;
    [ObservableProperty] string message;


    public LoginViewModel(DatabaseService db, MainPage mainPage)
    {
        _db = db;
        _mainPage = mainPage;
    }

    [RelayCommand]
    public async Task Login()
    {
        bool valid = await _db.ValidateUserAsync(Email, Password);
        if (valid)
        {
            await Application.Current.MainPage.Navigation.PushAsync(_mainPage);
        }
        else
        {
            Message = "Login failed. Check Email or Password";
        }
    }
    [RelayCommand]
    public async Task Register()
    { 
    bool success = await _db.RegisterUserAsync(Email, Password);
    Message= success? "User made!" : "Email already exist";
        }
}

