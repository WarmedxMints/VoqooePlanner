using System.Windows;
using System.Windows.Controls;

namespace VoqooePlanner.Views
{
    /// <summary>
    /// Interaction logic for VoqooeListView.xaml
    /// </summary>
    public partial class VoqooeListView : UserControl
    {
        public VoqooeListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }
    }
}
