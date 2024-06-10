using System.Windows;
using System.Windows.Controls;
using VoqooePlanner.ViewModels.ModelViews;

namespace VoqooePlanner.Controls
{
    /// <summary>
    /// Interaction logic for OrganicScanDeatilsViewControl.xaml
    /// </summary>
    public partial class OrganicScanDeatilsViewControl : UserControl
    {

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(OrganicScanDeatilsViewControl), new PropertyMetadata(string.Empty));



        public List<OrganicScanDetailsViewModel> Species
        {
            get { return (List<OrganicScanDetailsViewModel>)GetValue(SpeciesProperty); }
            set { SetValue(SpeciesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Species.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpeciesProperty =
            DependencyProperty.Register("Species", typeof(List<OrganicScanDetailsViewModel>), typeof(OrganicScanDeatilsViewControl), new PropertyMetadata());



        public OrganicScanDeatilsViewControl()
        {
            InitializeComponent();
        }
    }
}
