using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfGui
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        private static ReportWindow _reportWindow;
        public static ReportWindow GetReportWindow()
        {
            if (_reportWindow == null) _reportWindow = new ReportWindow();
            return _reportWindow;
        }

        public ReportWindow()
        {
            InitializeComponent();

            this.Title = ConstantNames.ApplicationFullName + " - Report";
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
