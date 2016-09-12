using Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        private ProjectsManager ProjectsManager
        {
            get;
            set;
        }
        private HistoryProvider HistoryProvider
        {
            get;
            set;
        }

        private static ReportWindow _reportWindow;
        public static ReportWindow GetReportWindow(ProjectsManager ProjectsManager)
        {
            if (_reportWindow == null) _reportWindow = new ReportWindow(ProjectsManager);
            _reportWindow.PopulateProjectsSelector();
            return _reportWindow;
        }
        private void PopulateProjectsSelector()
        {

        }

        #region -> Constructors <-

        public ReportWindow(ProjectsManager ProjectsManager)
        {
            Contract.Requires(ProjectsManager != null);

            InitializeComponent();

            this.ProjectsManager = ProjectsManager;
            this.HistoryProvider = new HistoryProvider(ProjectsManager);
            this.Title = ConstantNames.ApplicationFullName + " - Report";
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime nowDate = DateTime.Now;
            dtpFromSelector.DisplayDate = nowDate;
            dtpToSelector.DisplayDate = nowDate;
        }
    }
}
