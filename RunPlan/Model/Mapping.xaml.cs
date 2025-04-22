using System.Collections.Specialized;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;


namespace RunPlan.Model;
public partial class Mapping : ContentPage
{
    
    public Mapping(MappingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Subscribe to RouteUpdated event to refresh GraphicsView
        viewModel.RouteUpdated += () =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                RouteGraphicsView.Invalidate();  // Refresh drawing
            });
        };
    }
}