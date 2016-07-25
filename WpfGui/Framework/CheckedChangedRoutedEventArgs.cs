using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfGui.Framework
{
    internal class CheckedChangedRoutedEventArgs:RoutedEventArgs
    {
        public bool IsChecked
        {
            get;
            private set;
        }

        public CheckedChangedRoutedEventArgs(bool IsChecked)
        {
            this.IsChecked = IsChecked;
        }
        public CheckedChangedRoutedEventArgs(RoutedEvent RoutedEvent, bool IsChecked) : base(RoutedEvent)
        {
            this.IsChecked = IsChecked;
        }
        public CheckedChangedRoutedEventArgs(RoutedEvent RoutedEvent, object Source, bool IsChecked):base(RoutedEvent, Source)
        {
            this.IsChecked = IsChecked;
        }
    }
}
