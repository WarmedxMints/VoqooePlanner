﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VoqooePlanner.Views
{
    /// <summary>
    /// Interaction logic for OrganicCheckListView.xaml
    /// </summary>
    public partial class OrganicCheckListView : UserControl
    {
        public OrganicCheckListView()
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
