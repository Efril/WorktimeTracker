using Core;
using Core.Framework;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using WpfGui.Properties;
using WpfGui.Framework;

namespace WpfGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ContextMenu trayMenu;
        private System.Windows.Forms.NotifyIcon _trayIcon = new System.Windows.Forms.NotifyIcon();
        private ProjectsManager _projectsManager;
        
        public MainWindow()
        {
            InitializeComponent();

            this.Visibility = Visibility.Hidden;

            InitializeTrayIcon();
            _projectsManager = new ProjectsManager();
        }

        #region -> Tray icon and menu logic <-

        private void InitializeTrayIcon()
        {
            trayMenu = FindResource("trayMenu") as ContextMenu;
            _trayIcon.DoubleClick += _trayIcon_DoubleClick;
            _trayIcon.MouseDown += _trayIcon_MouseDown;
            _trayIcon.Icon = WpfGui.Properties.Resources.ClockStopped;
            _trayIcon.Visible = true;
        }

        private void _trayIcon_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                //Open context menu
                trayMenu.IsOpen = true;
            }
        }
        private void _trayIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowForm();
        }
        private void showHideMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(this.IsVisible)
            {
                MenuItem showHideMenuItem = e.OriginalSource as MenuItem;
                this.Hide();
                showHideMenuItem.Header = "Show";
            }
            else
            {
                ShowForm();
            }
        }
        private void ShowForm()
        {
            MenuItem showHideMenuItem = (MenuItem)LogicalTreeHelper.FindLogicalNode(trayMenu, "showHideMenuItem");
            this.Show();
            this.Focus();
            this.Activate();
            showHideMenuItem.Header = "Hide";
        }
        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _trayIcon.Dispose();
            Application.Current.Shutdown();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized) this.Hide();

            base.OnStateChanged(e);
        }

        #endregion

        private void ContextMenu_LostFocus(object sender, RoutedEventArgs e)
        {
            ContextMenu trayMenu = e.OriginalSource as ContextMenu;
            trayMenu.IsOpen = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeProjectSelectorBlock();
            
            LayoutWindow();
        }
        private void InitializeProjectSelectorBlock()
        {
            projectSelector.Initialize(_projectsManager);
            if (projectSelector.SelectedProject != null) lblSelectedProjectName.PerformClick();
        }
        private void LayoutWindow()
        {
            double locationTop, locationLeft;
            LayoutManager.GetAboveTrayNotificationAreaWindowPosition(this.Width, this.Height, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight, out locationTop, out locationLeft);
            this.Top = locationTop;
            this.Left = locationLeft;
        }

        private void imageButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void lblSelectedProjectName_Click(object sender, RoutedEventArgs e)
        {
            projectSelector.Click();
            EnsureProjectSelectorVisible();
        }

        private void projectSelector_SomethingWentWrong(object sender, SomethingWentWrongEventArgs e)
        {
            DisplayErrorMessage(e.Message);
        }
        private void projectSelector_BeforeAnyAction(object sender, EventArgs e)
        {
            EnsureErrorMessageCollapsed();
        }

        private void DisplayErrorMessage(string Message)
        {
            lblErrorMessage.Content = Message;
            lblErrorMessage.Visibility = Visibility.Visible;
        }
        private void EnsureErrorMessageCollapsed()
        {
            lblErrorMessage.Visibility = Visibility.Collapsed;
        }
        private void projectSelector_SelectedProjectChanged(object sender, EventArgs e)
        {
            TimeSpan worktimeToDisplay;
            if(projectSelector.SelectedProject!=null)
            {
                EnsureProjectSelectorVisible();
                panelWorktimeBlock.IsEnabled = true;
                worktimeToDisplay = projectSelector.SelectedProject.TimeTracker.GetElapsedToday();
            }
            else
            {
                panelWorktimeBlock.IsEnabled = false;
                worktimeToDisplay = new TimeSpan();
            }
            DisplayElapsedWorktime(worktimeToDisplay);
        }
        private void EnsureProjectSelectorVisible()
        {
            projectSelector.Visibility = Visibility.Visible;
            lblSelectedProjectName.Visibility = Visibility.Collapsed;
        }
        private void DisplayCurrentProjectElapsedWorktime()
        {
            DisplayElapsedWorktime(projectSelector.SelectedProject.TimeTracker.GetElapsedToday());
        }
        private void DisplayElapsedWorktime(TimeSpan ElapsedWorktime)
        {
            string worktimeString = ElapsedWorktime.ToString(@"hh\:mm");
            lblWorktimeElapsed.Text = worktimeString;
            lblWorktimeElapsed.ToolTip = worktimeString + " elapsed today";
        }
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute= projectSelector.SelectedProject != null;
        }
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MenuItem startStopMenuItem = (MenuItem)LogicalTreeHelper.FindLogicalNode(trayMenu, "startStopTimerMenuItem");
            if (projectSelector.SelectedProject.TimeTracker.Running)
            {
                projectSelector.SelectedProject.TimeTracker.Stop();
                projectSelector.SelectedProject.TimeTracker.Heatbeat -= TimeTracker_Heatbeat;
                DisplayCurrentProjectElapsedWorktime();
                btnStartStopCounting.Image = new BitmapImage(new Uri("Images/Start.png", UriKind.Relative));
                btnStartStopCounting.ToolTip = "Start counting worktime";
                _trayIcon.Icon = Properties.Resources.ClockStopped;
                startStopMenuItem.Header = "Start";
            }
            else
            {
                projectSelector.SelectedProject.TimeTracker.Heatbeat += TimeTracker_Heatbeat;
                projectSelector.SelectedProject.TimeTracker.Start();
                btnStartStopCounting.Image = new BitmapImage(new Uri("Images/Stop.png", UriKind.Relative));
                btnStartStopCounting.ToolTip = "Stop counting worktime";
                _trayIcon.Icon = Properties.Resources.ClockRunning;
                startStopMenuItem.Header = "Stop";
                Properties.Settings.Default.LastProjectName = projectSelector.SelectedProject.Name;
                Properties.Settings.Default.Save();
            }
        }

        private void TimeTracker_Heatbeat(object sender, ElapsedTodaEventArgs e)
        {
            this.Dispatcher.Invoke(new Action<TimeSpan>(DisplayElapsedWorktime), (object)e.ElapsedToday);
        }
    }
}
