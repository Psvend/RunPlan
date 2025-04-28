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
}