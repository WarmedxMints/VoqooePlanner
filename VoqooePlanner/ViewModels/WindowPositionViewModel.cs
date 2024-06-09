using System.Windows;
using VoqooePlanner.Migrations;

namespace VoqooePlanner.ViewModels
{
    public sealed class WindowPositionViewModel : ViewModelBase
    {
        private double top;
        private double left;
        private double height;
        private double width;
        private WindowState state = WindowState.Normal;

        public double Top { get => top; set { top = value; OnPropertyChanged(); } }
        public double Left { get => left; set { left = value; OnPropertyChanged(); } }
        public double Height { get => height; set { height = value; OnPropertyChanged(); } }
        public double Width { get => width; set { width = value; OnPropertyChanged(); } }
        public WindowState State { get => state; set { state = value; OnPropertyChanged(); } }

        public bool IsZero => Top == 0 && Left == 0 && Height == 0 && Width == 0;
    }
}
