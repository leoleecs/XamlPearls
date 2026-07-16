using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace XamlPearls.ValueConverters
{
    internal class StringBoolConverter : IValueConverter
    {
        public bool Reverse { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var @bool = String.IsNullOrWhiteSpace(System.Convert.ToString(value, culture));
            return Reverse ? !@bool : @bool;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
