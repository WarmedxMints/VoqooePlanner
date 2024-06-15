using System.Windows.Controls;
using VoqooePlanner.ViewModels.MainViews;
using VoqooePlanner.ViewModels.ModelViews;

namespace VoqooePlanner.Controls
{
    /// <summary>
    /// Interaction logic for CartoBodyDetailsControl.xaml
    /// </summary>
    public partial class CartoBodyDetailsControl : UserControl
    {
        public CartoBodyDetailsControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is CartoDataViewModel model)
            {
                model.OnSelectedBodyChanged += OnSelectedBodyChanged;
            }
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is CartoDataViewModel model)
            {
                model.OnSelectedBodyChanged -= OnSelectedBodyChanged;
            }
        }

        private void OnSelectedBodyChanged(object? sender, SystemBodyViewModel? e)
        {
            if (e != null)
                BodiesGrid.ScrollIntoView(e);
        }
    }
}
