using System.Windows.Controls;
using VoqooePlanner.ViewModels.MainViews;
using VoqooePlanner.ViewModels.ModelViews;

namespace VoqooePlanner.Controls
{
    /// <summary>
    /// Interaction logic for CartoSystemListControl.xaml
    /// </summary>
    public partial class CartoSystemListControl : UserControl
    {
        public CartoSystemListControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if(DataContext is CartoDataViewModel model)
            {
                model.OnSelectedSystemChanged += OnSelectedSystemChanged;
            }
        }
        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is CartoDataViewModel model)
            {
                model.OnSelectedSystemChanged -= OnSelectedSystemChanged;
            }
        }

        private void OnSelectedSystemChanged(object? sender, StarSystemViewModel? e)
        {
            if(e != null)
                SystemGrid.ScrollIntoView(e);
        }
    }
}
