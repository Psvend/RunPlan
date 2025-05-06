namespace RunPlan.Model;

public partial class LoginPage : ContentPage

	
{ 
	public LoginViewModel ViewModel { get; }
	public LoginPage(LoginViewModel vm)
	{
		InitializeComponent();
		ViewModel = vm;
		BindingContext = vm;
	}


    protected override void OnAppearing()
    {
        base.OnAppearing();
		ViewModel.Email = string.Empty;
		ViewModel.Password = string.Empty;
		ViewModel.Message = string.Empty;
    }
}