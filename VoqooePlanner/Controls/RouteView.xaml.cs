using ODUtils.Helpers;
using System.Windows;
using System.Windows.Controls;
using VoqooePlanner.ViewModels.MainViews;
namespace VoqooePlanner.Controls
{
    /// <summary>
    /// Interaction logic for RouteView.xaml
    /// </summary>
    public partial class RouteView : UserControl
    {
        public RouteView()
        {
            InitializeComponent();
            Loaded += RouteView_Loaded;
            Unloaded += RouteView_Unloaded;
        }

        private void RouteView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is VoqooeListViewModel model)
            {
                model.OnStringCopiedToClipboard += OnStringCopiedToClipboard;
                model.OnRouteCreated += OnRouteCreated;
            }
        }

        private void OnRouteCreated(object? sender, System.EventArgs e)
        {
            this.Focus();
        }

        private void RouteView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is VoqooeListViewModel model)
            {
                model.OnStringCopiedToClipboard -= OnStringCopiedToClipboard;
                model.OnRouteCreated -= OnRouteCreated;
            }
        }

        private void OnStringCopiedToClipboard(object? sender, string e)
        {
            ClipboardText.Text = $"{e} copied to clipboard";
            WPFUiEffects.FadeInOutElement(ClipboardText);
        }
    }
}
