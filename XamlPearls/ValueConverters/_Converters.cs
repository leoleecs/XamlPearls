using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace XamlPearls.ValueConverters
{
    public static class _Converters
    {
        public static readonly IValueConverter TrueVisibleFalseCollapsed;
        public static readonly IValueConverter TrueVisibleFalseHidden;
        public static readonly IValueConverter FalseVisibleTrueCollapsed;
        public static readonly IValueConverter FalseVisibleTrueHidden;
        public static readonly IValueConverter NullVisibleNotNullCollapsed;
        public static readonly IValueConverter NullVisibleNotNullHidden;
        public static readonly IValueConverter NotNullVisibleNullCollapsed;
        public static readonly IValueConverter NotNullVisibleNullHidden;
        public static readonly IValueConverter EnumToTrue;// A | B | C => A => true, B => true, C => true, D => false
        public static readonly IValueConverter EnumToFalse;
        public static readonly IValueConverter EnumToVisible;
        public static readonly IValueConverter EnumToCollapsed;
        public static readonly IValueConverter EnumToHidden;
        public static readonly IValueConverter EnumToInt;
        public static readonly IValueConverter EnumToString;

        public static readonly IValueConverter EmptyOrWhiteSpaceToTrue;
        public static readonly IValueConverter EmptyOrWhiteSpaceToFalse;
        public static readonly IValueConverter EmptyOrWhiteSpaceToVisible;
        public static readonly IValueConverter EmptyOrWhiteSpaceToVisibleNotHidden;
        public static readonly IValueConverter EmptyOrWhiteSpaceToHidden;
        public static readonly IValueConverter EmptyOrWhiteSpaceToCollapsed;

        static _Converters()
        {
            EmptyOrWhiteSpaceToTrue = new StringBoolConverter();
            EmptyOrWhiteSpaceToFalse = new StringBoolConverter() { Reverse = true };
            EmptyOrWhiteSpaceToVisible = new StringVisibilityConverter()
            {
                TrueValue = Visibility.Visible,
                FalseValue = Visibility.Collapsed
            };
            EmptyOrWhiteSpaceToVisibleNotHidden = new StringVisibilityConverter()
            {
                TrueValue = Visibility.Visible,
                FalseValue = Visibility.Hidden
            };
            EmptyOrWhiteSpaceToHidden = new StringVisibilityConverter()
            {
                TrueValue = Visibility.Hidden,
                FalseValue = Visibility.Visible
            };
            EmptyOrWhiteSpaceToCollapsed = new StringVisibilityConverter()
            {
                TrueValue = Visibility.Collapsed,
                FalseValue = Visibility.Visible
            };
        }
    }
}