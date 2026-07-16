using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace XamlPearls.XamlFont
{
    public class FontSizeListBoxItemToDoubleConverter : IValueConverter
    {
        public FontSizeListBoxItemToDoubleConverter()
        {
        }

        object System.Windows.Data.IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value.ToString();
            try
            {
                return double.Parse(value.ToString());
            }
            catch (FormatException ex)
            {
                return 0;
            }

        }

        object System.Windows.Data.IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
