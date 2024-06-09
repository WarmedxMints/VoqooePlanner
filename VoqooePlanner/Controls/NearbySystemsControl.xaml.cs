using System.Windows;
using System.Windows.Controls;
using VoqooePlanner.ViewModels;

namespace VoqooePlanner.Controls
{
    /// <summary>
    /// Interaction logic for NearbySystemsControl.xaml
    /// </summary>
    public partial class NearbySystemsControl : UserControl
    {
        public NearbySystemsControl()
        {
            InitializeComponent();
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is VoqooeListViewModel model)
            {
                model.RefreshGrid += OnRefeshGrid;
            }
        }

        private void OnRefeshGrid()
        {
            SystemGrid.Items.Refresh();
        }

        private void DataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is VoqooeListViewModel model)
            {
                model.RefreshGrid -= OnRefeshGrid;
            }
        }
    }
}