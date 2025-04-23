namespace RunPlan
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ActivityDetail), typeof(ActivityDetail));
        }
    }
}
