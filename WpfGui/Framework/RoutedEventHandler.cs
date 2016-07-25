using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfGui.Framework
{
    internal delegate void RoutedEventHandler<TEventArgs>(object sender, TEventArgs Args) where TEventArgs : RoutedEventArgs;
}
