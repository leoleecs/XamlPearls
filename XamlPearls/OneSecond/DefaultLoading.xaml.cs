using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace XamlPearls.OneSecond
{
    /// <summary>
    /// DefaultLoading.xaml 的交互逻辑
    /// </summary>
    public partial class DefaultLoading : UserControl
    {
        // Using a DependencyProperty as the backing store for FillColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillColorProperty =
            DependencyProperty.Register("FillColor", typeof(Color), typeof(DefaultLoading), new PropertyMetadata(Colors.Black));

        public DefaultLoading()
        {
            InitializeComponent();
        }

        public Color FillColor
        {
            get { return (Color)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }
    }
}