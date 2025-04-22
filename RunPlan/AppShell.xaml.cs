namespace RunPlan
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("ActivityDetail", typeof(ActivityDetail));
         
        }
    }
}
