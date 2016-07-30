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

namespace WpfGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            _trayIcon.DoubleClick += _trayIcon_DoubleClick;
            _trayIcon.MouseDown += _trayIcon_MouseDown;
            _trayIcon.Icon = WpfGui.Properties.Resources.ClockRunning;
            _trayIcon.Visible = true;
        }

        private void _trayIcon_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                //Open context menu
                ContextMenu trayMenu = FindResource("trayMenu") as ContextMenu;
                trayMenu.IsOpen = true;
            }
        }
        private void _trayIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.Focus();
            this.Activate();
        }
        private void showHideMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem showHideMenuItem = e.OriginalSource as MenuItem;
            if(this.IsVisible)
            {
                this.Hide();
                showHideMenuItem.Header = "Show";
            }
            else
            {
                this.Show();
                showHideMenuItem.Header = "Hide";
            }
        }
        private void startStopTimerMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
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
            projectSelector.Initialize(_projectsManager);
            
            LayoutWindow();
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
            lblSelectedProjectName.Visibility = Visibility.Collapsed;
            projectSelector.Click();
        }

        private void projectSelector_SomethingWentWrong(object sender, SomethingWentWrongEventArgs e)
        {
            DisplayErrorMessage(e.Message);
        }
        private void projectSelector_BeforeAnyAction(object sender, EventArgs e)
        {
            EnsureErrorMessageHidden();
        }

        private void DisplayErrorMessage(string Message)
        {
            lblErrorMessage.Content = Message;
            lblErrorMessage.Visibility = Visibility.Visible;
        }
        private void EnsureErrorMessageHidden()
        {
            lblErrorMessage.Visibility = Visibility.Hidden;
        }
        private void projectSelector_SelectedProjectChanged(object sender, EventArgs e)
        {
            TimeSpan worktimeToDisplay;
            if(projectSelector.SelectedProject!=null)
            {
                panelWorktimeBlock.IsEnabled = true;
                worktimeToDisplay = projectSelector.SelectedProject.TimeTracker.GetElapsedToday();
            }
            else
            {
                panelWorktimeBlock.IsEnabled = false;
                worktimeToDisplay = new TimeSpan();
            }

            string worktimeString = worktimeToDisplay.ToString(@"hh\:mm");
            lblWorktimeElapsed.Text = worktimeString;
            lblWorktimeElapsed.ToolTip = worktimeString + " elapsed today";
        }
        private void btnStartStopCounting_Click(object sender, RoutedEventArgs e)
        {
            if (projectSelector.SelectedProject.TimeTracker.Running)
            {
                projectSelector.SelectedProject.TimeTracker.Stop();
                btnStartStopCounting.Image = (ImageSource)new ImageSourceConverter().ConvertFromString("Images/Start.png");
                btnStartStopCounting.ToolTip = "Start counting worktime";
                _trayIcon.Icon = Properties.Resources.ClockStopped;
            }
            else
            {
                projectSelector.SelectedProject.TimeTracker.Start();
                btnStartStopCounting.Image = (ImageSource)new ImageSourceConverter().ConvertFromString("Images/Stop.png");
                btnStartStopCounting.ToolTip = "Stop counting worktime";
                _trayIcon.Icon = Properties.Resources.ClockRunning;
            }
        }
    }
}
