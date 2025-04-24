using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunPlan.Model
{
    public partial class CreateTraining : ContentPage
    {

        private readonly CreateTrainingViewModel _viewModel;


        public CreateTraining(CreateTrainingViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            BindingContext = _viewModel;

            //Triggers training to load manually
            Loaded += async (_, _) => await _viewModel.LoadTrainingsAsync();

        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//TrainingList");
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_viewModel.LoadTrainingsCommand.CanExecute(null))
                _viewModel.LoadTrainingsCommand.Execute(null);
        }



    }

}
