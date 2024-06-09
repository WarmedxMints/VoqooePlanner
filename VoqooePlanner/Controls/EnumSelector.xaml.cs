using ODUtils.Extensions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VoqooePlanner.Controls
{
    /// <summary>
    /// Interaction logic for EnumSelector.xaml
    /// </summary>
    public partial class EnumSelector : Button
    {
        private ObservableCollection<MenuItem> menuItems = [];
        public ObservableCollection<MenuItem> MenuItems { get => menuItems; set => menuItems = value; }

        public ICommand OnMenuClosed
        {
            get { return (ICommand)GetValue(OnMenuClosedProperty); }
            set { SetValue(OnMenuClosedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnMenuClosedProperty =
            DependencyProperty.Register("OnMenuClosed", typeof(ICommand), typeof(EnumSelector), new PropertyMetadata());

        public List<int> SelectedValues
        {
            get { return (List<int>)GetValue(SelectedValuesProperty); }
            set { SetValue(SelectedValuesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedValues.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedValuesProperty =
            DependencyProperty.Register("SelectedValues", typeof(List<int>), typeof(EnumSelector), new PropertyMetadata());



        public IEnumerable BackingEnum
        {
            get { return (IEnumerable)GetValue(BackingEnumProperty); }
            set { SetValue(BackingEnumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackingEnum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackingEnumProperty =
            DependencyProperty.Register("BackingEnum", typeof(IEnumerable), typeof(EnumSelector), new PropertyMetadata());


        public EnumSelector()
        {
            InitializeComponent();
        }

        private void BuildMenuOptions()
        {
            if (BackingEnum is null)
            {
                return;
            }

            MenuItem selectallItem = new()
            {
                Header = "Select All",
                IsCheckable = true,
                StaysOpenOnClick = true,
                Tag = -1,
                IsChecked = SelectedValues.Contains(-1)
            };

            selectallItem.Click += Menuitem_Click;
            menuItems.Add(selectallItem);

            var idx = 0;

            foreach (var item in BackingEnum)
            {
                if (item is not Enum enumType)
                    continue;

                MenuItem menuitem = new()
                {
                    Header = enumType.GetDescription(),
                    IsCheckable = true,
                    StaysOpenOnClick = true,
                    IsChecked = SelectedValues.Contains(idx) || SelectedValues.Contains(-1),
                    Tag = idx,
                };
                menuitem.Click += Menuitem_Click;
                menuItems.Add(menuitem);
                idx++;
            }

            menu.ItemsSource = menuItems;
        }
        private void Menuitem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item)
                return;

            int index = (int)item.Tag;

            if (index < 0)
            {
                var active = item.IsChecked;

                foreach (MenuItem mItem in menuItems)
                {
                    if (mItem == item)
                    {
                        continue;
                    }
                    mItem.IsChecked = active;
                }
            }
            else
            {
                MenuItem? selectAll = menuItems.FirstOrDefault(x => (int)x.Tag == -1);

                if(selectAll != null) 
                    selectAll.IsChecked = false;
            }
            e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // IsOpen will always be false here, but IsVisible will give us the menu state at the time the button was clicked.
            if (!menu.IsVisible && sender is Button btn)
            {
                if(menuItems.Count == 0)
                {
                    BuildMenuOptions();
                }
                menu.PlacementTarget = btn;
                menu.VerticalOffset = 5;
                menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            
                menu.IsOpen = true;
            }
        }

        private void Menu_Closed(object sender, RoutedEventArgs e)
        {
            SelectedValues.Clear();

            foreach (MenuItem mItem in menuItems)
            {
                if (mItem.Tag is not int index)
                    continue;

                if (mItem.IsChecked)
                    SelectedValues.Add(index);
            }

            OnMenuClosed?.Execute(this);
        }
    }
}
