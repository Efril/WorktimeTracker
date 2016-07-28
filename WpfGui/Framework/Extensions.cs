using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfGui.Framework
{
    public static class Extensions
    {
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
        public static void ForceFocus(this ComboBox ComboBox)
        {
            TextBox textBox = (ComboBox.Template.FindName("PART_EditableTextBox", ComboBox) as TextBox);
            if(textBox!=null && !ComboBox.IsDropDownOpen)
            {
                textBox.Focus();
            }
        }
    }
}
