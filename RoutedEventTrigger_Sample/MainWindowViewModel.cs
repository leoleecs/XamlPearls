using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RoutedEventTrigger_Sample
{
    class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            MoveCommand = new DelegateCommand<object>(ExecuteDelegateCommand);
        }

        private void ExecuteDelegateCommand(object obj)
        {
            var distance = double.Parse(obj.ToString());
            MessageBox.Show($"You have Moved {distance} cm.", "Distance Moved", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        public ICommand MoveCommand { get; set; }
    }
}
