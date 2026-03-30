using System.ComponentModel;
using System.Runtime.CompilerServices;
using OxyPlot;

namespace GeneClusteringWPFApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private PlotModel _plotModel;

        public PlotModel PlotModel
        {
            get => _plotModel;
            set
            {
                _plotModel = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}