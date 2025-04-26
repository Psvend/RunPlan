using Microsoft.Maui.Controls;
using RunPlan.ViewModel;
using RunPlan.Model;
using System.Collections.Generic;

namespace RunPlan.Model
{
    public partial class TrainingList : ContentPage
    {
        private readonly TrainingListViewModel _viewModel;

        public TrainingList(TrainingListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
                await _viewModel.LoadTrainingsAsync();
        }

        private async void OnAddTrainingClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//CreateTraining");
        }

        private async void OnTrainingSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Training selected)
            {
                await Shell.Current.GoToAsync("TrainingDetail", true, new Dictionary<string, object>
                {
                    { "Training", selected }
                });

                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private void OnDeleteTrainingClicked (object sender, EventArgs e)
        {
            if(sender is ImageButton button && button.CommandParameter is Training trainingToDelete)
            {
                _viewModel.DeleteTraining(trainingToDelete);
            }
        }
    }
}
