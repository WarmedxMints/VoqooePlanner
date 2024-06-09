using System.Globalization;
using System.Windows.Data;
using VoqooePlanner.Models;

namespace VoqooePlanner.WpfConverters
{
    public sealed class NearbyOptionsConvertor : IValueConverter
    {
        private NearBySystemsOptions target;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            NearBySystemsOptions mask = (NearBySystemsOptions)parameter;
            this.target = (NearBySystemsOptions)value;
            return ((mask & this.target) != 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            this.target ^= (NearBySystemsOptions)parameter;
            return this.target;
        }
    }
}