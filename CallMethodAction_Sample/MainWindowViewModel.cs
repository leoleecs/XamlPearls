using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CallMethodAction_Sample
{
    internal class MainWindowViewModel
    {
        public void OnSubmit(object firstName, string lastName)
        {
            MessageBox.Show($"Hello {firstName} {lastName}!");
        }

        public void ShowMouseDownTime(MouseEventArgs e)
        {
            MessageBox.Show($"Mouse down at {e.Timestamp} ms");
        }
    }
}
