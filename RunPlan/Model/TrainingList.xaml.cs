using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using RunPlan.ViewModel;




namespace RunPlan.Model
{
    public partial class TrainingList : ContentPage
    {
        private bool isNavigating = false;

        public TrainingList(TrainingListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }



        // Add training button clicked
        private async void OnAddTrainingClicked(object sender, EventArgs e)
        {
            if (isNavigating) return;
            isNavigating = true;

            await Shell.Current.GoToAsync("//CreateTraining"); // Route to your AddTraining page

            isNavigating = false;
        }



        // Refresh on appearing
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is TrainingListViewModel vm)
                await vm.LoadTrainingsAsync();
        }



        // Search bar updated
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (BindingContext is TrainingListViewModel vm)
            {
                vm.FilterTrainingsBySearch();
            }
        }

        // When a training is tapped
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
    }
}
