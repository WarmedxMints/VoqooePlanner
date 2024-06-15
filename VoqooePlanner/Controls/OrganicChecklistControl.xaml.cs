using System.Windows.Controls;
using System.Windows.Input;

namespace VoqooePlanner.Controls
{
    /// <summary>
    /// Interaction logic for OrganicChecklistControl.xaml
    /// </summary>
    public partial class OrganicChecklistControl : UserControl
    {
        public OrganicChecklistControl()
        {
            InitializeComponent();
        }

        private void Panels_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainScrollBar.ScrollToVerticalOffset(MainScrollBar.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
