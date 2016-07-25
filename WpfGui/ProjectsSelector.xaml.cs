using Core.Framework;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Core;
using System.Diagnostics.Contracts;
using WpfGui.Properties;

namespace WpfGui
{
    /// <summary>
    /// Interaction logic for Projects.xaml
    /// </summary>
    public partial class ProjectsSelector : UserControl
    {
        private ProjectsManager _projectsManager;

        public Project SelectedProject
        {
            get { return cbProjects.SelectedItem as Project; }
        }

        public event EventHandler<SomethingWentWrongEventArgs> SomethingWentWrong;
        private void OnSomethingWentWrong(string Message)
        {
            if (SomethingWentWrong != null) SomethingWentWrong(this, new SomethingWentWrongEventArgs(Message));
        }

        public void Initialize(ProjectsManager ProjectsManager)
        {
            Contract.Requires(ProjectsManager != null);
            _projectsManager = ProjectsManager;
        }

        public void Click()
        {
            if(cbProjects.SelectedItem==null && cbProjects.Items.Count==0)
            {
                //In case if no project created yet switch projects combobox to edit mode to allow user to create new one
                btnAddNewProject.IsChecked = true;
            }
            this.Visibility = Visibility.Visible;
        }

        public ProjectsSelector()
        {
            InitializeComponent();

            btnAddNewProject.CheckedChanged += BtnAddNewProject_CheckedChanged;
            btnRenameProject.CheckedChanged += BtnRenameProject_CheckedChanged;
            btnDeleteProject.Click += BtnDeleteProject_Click;
        }

        #region -> Handling buttons <-

        private void BtnDeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProject != null && MessageBox.Show("Do you really want to delete '" + SelectedProject.Name + "' project? All worktime tracking data will be removed also.", string.Empty, MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                MethodCallResult projectDeleted = _projectsManager.DeleteProject(SelectedProject.Name);
                if (projectDeleted) cbProjects.Items.Remove(SelectedProject);
            }
        }
        private void BtnRenameProject_CheckedChanged(object sender, RoutedEventArgs e)
        {
            cbProjects.IsEditable = true;
        }
        private void BtnAddNewProject_CheckedChanged(object sender, RoutedEventArgs e)
        {
            cbProjects.IsEditable = true;
        }

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadProjects();
        }
        private bool ReloadProjects()
        {
            Settings.Default.Reload();
            Project[] projects;
            MethodCallResult getProjectsResult = _projectsManager.GetAllProjects(out projects);
            if(!getProjectsResult)
            {
                OnSomethingWentWrong(getProjectsResult.ToString());
                return false;
            }
            cbProjects.ItemsSource = projects;
            if(!string.IsNullOrWhiteSpace(Settings.Default.LastProjectName))
            {
                Project selectedProject = projects.FirstOrDefault(p => string.Equals(p.Name, Settings.Default.LastProjectName));
                if (selectedProject != null) cbProjects.SelectedItem = selectedProject;
            }
            return true;
        }

        private void cbProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Visibility buttonsVisible = cbProjects.SelectedItem != null ? Visibility.Visible : Visibility.Hidden;
            btnDeleteProject.Visibility = buttonsVisible;
            btnRenameProject.Visibility = buttonsVisible;
        }
    }
}
