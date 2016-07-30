using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfGui.Framework
{
    public static class Extensions
    {
        public static void DoEvents(this Application Application)
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }
        private static object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }

        public static void PerformClick(this Button btn)
        {
            btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        public static void SelectAll(this ComboBox ComboBox)
        {
            TextBox textBox = (ComboBox.Template.FindName("PART_EditableTextBox", ComboBox) as TextBox);
            if (textBox != null && !ComboBox.IsDropDownOpen)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    textBox.SelectAll();
                    textBox.Focus();
                }));
            }
        }
    }
}
