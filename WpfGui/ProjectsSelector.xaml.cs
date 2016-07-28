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
using WpfGui.Framework;

namespace WpfGui
{
    /// <summary>
    /// Interaction logic for Projects.xaml
    /// </summary>
    public partial class ProjectsSelector : UserControl
    {
        private bool _initialized
        {
            get { return _projectsManager != null; }
        }
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

            //TextBox box = cbProjects.Template.FindName("PART_EditableTextBox", cbProjects) as TextBox;
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
            cbProjects.SelectAll();
        }
        private void BtnAddNewProject_CheckedChanged(object sender, RoutedEventArgs e)
        {
            cbProjects.IsEditable = true;
            cbProjects.ForceFocus();
        }

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(_initialized) ReloadProjects();
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
        private void cbProjects_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (cbProjects.IsEditable)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        {
                            MethodCallResult operationSuccessfull;
                            if (cbProjects.SelectedItem != null)
                            {
                                //project about to be renamed
                                operationSuccessfull= _projectsManager.RenameProject(cbProjects.SelectedItem as Project, cbProjects.Text.Trim());
                                if (operationSuccessfull)
                                {
                                    cbProjects.IsEditable = false;
                                    btnRenameProject.IsChecked = false;
                                }
                            }
                            else
                            {
                                //project about to be added
                                Project createdProject;
                                operationSuccessfull = _projectsManager.CreateProject(cbProjects.Text.Trim(), out createdProject);
                                if(operationSuccessfull)
                                {
                                    cbProjects.IsEditable = false;
                                    Project[] projectsSource;
                                    _projectsManager.GetAllProjects(out projectsSource);
                                    cbProjects.ItemsSource = projectsSource;
                                    cbProjects.SelectedItem = createdProject;
                                    btnAddNewProject.IsChecked = false;
                                }
                            }
                            if(!operationSuccessfull)
                            {
                                OnSomethingWentWrong(operationSuccessfull.ToString());
                            }
                            break;
                        }
                    case Key.Escape:
                        {
                            cbProjects.IsEditable = false;
                            btnAddNewProject.IsChecked = false;
                            btnRenameProject.IsChecked = false;
                            break;
                        }
                }
            }
        }

        private void cbProjects_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox box = cbProjects.Template.FindName("PART_EditableTextBox", cbProjects) as TextBox;
        }
    }
}
