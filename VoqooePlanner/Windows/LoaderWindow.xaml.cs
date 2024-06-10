using System.Windows;
using VoqooePlanner.ViewModels.MainViews;

namespace VoqooePlanner.Windows
{
    /// <summary>
    /// Interaction logic for LoaderWindow.xaml
    /// </summary>
    public partial class LoaderWindow : Window
    {
        public LoaderWindow()
        {
            InitializeComponent();
            Loaded += Loader_Loaded;
        }

        private void Loader_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoaderViewModel viewModel)
            {
                viewModel.OnUpdateComplete += OnUpdateCompete;
                _ = Task.Run(() => viewModel.CheckForUpdates(this));
                return;
            }
            this.Close();
        }

        private void OnUpdateCompete()
        {
            Application.Current.Dispatcher.Invoke(this.Close);
        }
    }
}