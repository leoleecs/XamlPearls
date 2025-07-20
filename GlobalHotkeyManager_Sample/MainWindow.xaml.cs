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
using XamlPearls.Shortcuts;

namespace GlobalHotkeyManager_Sample
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

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // Register a global hotkey (Ctrl + Shift + A)
            var hotKeyModel = new HotKeyModel("Ctrl+Shift+A", true, true, false, false, Keys.A);
            this.RegisterGlobalHotKey(hotKeyModel, OnHotKeyPressed);
        }

        private void OnHotKeyPressed(HotKeyModel model)
        {
            MessageBox.Show($"Hotkey pressed: {model.Name} ({model.Key})\nModifiers: {model.GetModifierKeys()}",
                "Global Hotkey Triggered", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}