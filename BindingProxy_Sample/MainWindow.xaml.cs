using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BindingProxy_Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public string Display
        {
            get { return (string)GetValue(DisplayProperty); }
            set { SetValue(DisplayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Display.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayProperty =
            DependencyProperty.Register("Display", typeof(string), typeof(MainWindow), new PropertyMetadata("MainWindow"));

    }
}