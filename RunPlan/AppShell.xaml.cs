namespace RunPlan
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("ActivityDetail", typeof(ActivityDetail));
            Routing.RegisterRoute("TrainingDetail", typeof(TrainingDetailPage));
        }
    }
}
