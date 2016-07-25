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
        System.Windows.Forms.NotifyIcon _trayIcon = new System.Windows.Forms.NotifyIcon();

        public MainWindow()
        {
            InitializeComponent();

            this.Visibility = Visibility.Hidden;

            InitializeTrayIcon();
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
    }
}
