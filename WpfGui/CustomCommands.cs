using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfGui
{
    internal static class CustomCommands
    {
        public static readonly RoutedUICommand SwitchStartStopWorktimeCounting = new RoutedUICommand("SwitchStartStopWorktimeCounting", "SwitchStartStopWorktimeCounting", typeof(CustomCommands));
    }
}
