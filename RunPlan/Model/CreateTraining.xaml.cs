using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunPlan.Model
{
    public partial class CreateTraining : ContentPage
    {
        public CreateTraining(CreateTrainingViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//TrainingList");
        }
    }

}
