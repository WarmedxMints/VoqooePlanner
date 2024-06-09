using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VoqooePlanner.Controls
{
    /// <summary>
    /// Interaction logic for ImageWithText.xaml
    /// </summary>
    public partial class ImageWithText : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ImageWithText), new PropertyMetadata(string.Empty));


        public ImageSource ImageURI
        {
            get { return (ImageSource)GetValue(ImageURIProperty); }
            set { SetValue(ImageURIProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageURI.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageURIProperty =
            DependencyProperty.Register("ImageURI", typeof(ImageSource), typeof(ImageWithText), new PropertyMetadata());


        public ImageWithText()
        {
            InitializeComponent();
        }
    }
}
