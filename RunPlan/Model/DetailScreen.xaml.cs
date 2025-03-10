namespace RunPlan.Model;

public partial class DetailScreen : ContentPage
{
	public DetailScreen(DetailViewModel detailViewModel)
	{
		InitializeComponent();
        BindingContext = detailViewModel;	
    }
}