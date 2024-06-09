using ODUtils.Helpers;
using System.Windows;
using System.Windows.Controls;
using VoqooePlanner.ViewModels;

namespace VoqooePlanner.Controls
{
    /// <summary>
    /// Interaction logic for StatusBarControl.xaml
    /// </summary>
    public partial class StatusBarControl : UserControl
    {
        public StatusBarControl()
        {
            InitializeComponent();
            Loaded += StatusBarControl_Loaded;
            Unloaded += StatusBarControl_Unloaded;
        }

        private void StatusBarControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.OnSystemsUpdateEvent -= OnSystemsUpdate;
            }
        }

        private void StatusBarControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.OnSystemsUpdateEvent += OnSystemsUpdate;
            }
        }

        private void OnSystemsUpdate(object? sender, bool updateStart)
        {
            //This will be called from another thread so invoke it on the main one
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (updateStart)
                {
                    WPFUiEffects.FadeInElement(UpdateTextBlock, 1.5);
                    return;
                }
                WPFUiEffects.FadeOutElement(UpdateTextBlock, 1.5);
            });
        } 
    }
}
