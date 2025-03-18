using System.Collections.Specialized;
using Microsoft.Maui.Graphics;

namespace RunPlan.Model;
public partial class Mapping : ContentPage
{
    private readonly MappingViewModel _viewModel;
    private readonly RouteDrawable _routeDrawable;
    private readonly GraphicsView _graphicsView;
    public Mapping(MappingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Subscribe to event to refresh GraphicsView
        viewModel.RouteUpdated += () => RouteGraphicsView.Invalidate();
    }
}