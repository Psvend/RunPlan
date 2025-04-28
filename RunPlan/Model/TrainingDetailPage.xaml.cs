using System.ComponentModel;
using Microsoft.Maui.Controls;
using RunPlan.ViewModel;
using RunPlan.Model;
using System;
using RunPlan.ViewModel;
using Microsoft.Maui.Controls.Xaml;




namespace RunPlan.Model
{
   
    public partial class TrainingDetailPage : ContentPage, INotifyPropertyChanged
    {
        private Training _training;
        public event PropertyChangedEventHandler PropertyChanged; 
        public TrainingListDetailViewModel ViewModel { get; }
        public TrainingDetailPage(TrainingListDetailViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;
        }
        protected new void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public Training training
        {
            get => ViewModel.Training;
            set => ViewModel.Training = value;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//TrainingList");
        }
      
    }
}

