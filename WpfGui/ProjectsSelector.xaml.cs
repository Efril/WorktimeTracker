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
using System.Windows.Threading;

namespace WpfGui
{
    /// <summary>
    /// Interaction logic for Projects.xaml
    /// </summary>
    public partial class ProjectsSelector : UserControl
    {
        enum ProjectsSelectorModes
        {
            Select, Add, Rename
        }

        private ProjectsSelectorModes _projectsSelectorMode = ProjectsSelectorModes.Select;
        private bool _initialized
        {
            get { return _projectsManager != null; }
        }
        private ProjectsManager _projectsManager;

        #region -> Interface <-

        private Project _selectedProject;
        public Project SelectedProject
        {
            get { return _selectedProject; }
            private set
            {
                Project originalProject = _selectedProject;
                _selectedProject = value;
                if(!Project.AreTheSame(originalProject, _selectedProject))
                {
                    OnSelectedProjectChanged();
                }
            }
        }
        public event EventHandler SelectedProjectChanged;
        private void OnSelectedProjectChanged()
        {
            if (SelectedProjectChanged != null) SelectedProjectChanged(this, new EventArgs());
        }

        public event EventHandler BeforeAnyAction;
        private void OnBeforeAnyAction()
        {
            if (BeforeAnyAction != null) BeforeAnyAction(this, new EventArgs());
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
            OnBeforeAnyAction();
            if(cbProjects.SelectedItem==null && cbProjects.Items.Count==0)
            {
                //In case if no project created yet switch projects combobox to edit mode to allow user to create new one
                btnAddNewProject.IsChecked = true;
            }
            this.Visibility = Visibility.Visible;
        }

        #endregion

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
            btnAddNewProject.IsChecked = false;
            btnRenameProject.IsChecked = false;
            if (SelectedProject != null && MessageBox.Show("Do you really want to delete '" + SelectedProject.Name + "' project? All worktime tracking data will be removed also.", string.Empty, MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                MethodCallResult projectDeleted = _projectsManager.DeleteProject(SelectedProject.Name);
                if (projectDeleted)
                {
                    Project[] projects;
                    AssignSelectorItemsSource(out projects);
                }
            }
        }
        private void BtnRenameProject_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if(btnRenameProject.IsChecked)
            {
                _projectsSelectorMode = ProjectsSelectorModes.Rename;
            }
            else
            {
                _projectsSelectorMode = ProjectsSelectorModes.Select;
            }
            SwitchComboBoxEditableMode(btnRenameProject);
        }
        private void BtnAddNewProject_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (btnAddNewProject.IsChecked)
            {
                cbProjects.SelectedIndex = -1;
                _projectsSelectorMode = ProjectsSelectorModes.Add;
            }
            else
            {
                _projectsSelectorMode = ProjectsSelectorModes.Select;
            }
            SwitchComboBoxEditableMode(btnAddNewProject);
        }
        private void SwitchComboBoxEditableMode(ImageButton Button)
        {
            OnBeforeAnyAction();
            cbProjects.IsEditable = Button.IsChecked;
            if (cbProjects.IsEditable)
            {
                Application.Current.DoEvents();
                cbProjects.SelectAll();
            }
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
            AssignSelectorItemsSource(out projects);
            if(!string.IsNullOrWhiteSpace(Settings.Default.LastProjectName))
            {
                Project selectedProject = projects.FirstOrDefault(p => string.Equals(p.Name, Settings.Default.LastProjectName));
                if (selectedProject != null) cbProjects.SelectedItem = selectedProject;
            }
            return true;
        }
        private MethodCallResult AssignSelectorItemsSource(out Project[] Projects)
        {
            MethodCallResult getProjectsResult = _projectsManager.GetAllProjects(out Projects);
            if (!getProjectsResult)
            {
                OnSomethingWentWrong(getProjectsResult.ToString());
            }
            cbProjects.ItemsSource = Projects;
            return getProjectsResult;
        }

        private void cbProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Visibility buttonsVisible = cbProjects.SelectedItem != null || _projectsSelectorMode== ProjectsSelectorModes.Rename ? Visibility.Visible : Visibility.Hidden;
            btnDeleteProject.Visibility = buttonsVisible;
            btnRenameProject.Visibility = buttonsVisible;
            if (_projectsSelectorMode != ProjectsSelectorModes.Rename) this.SelectedProject = cbProjects.SelectedItem as Project;
        }
        private void cbProjects_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (cbProjects.IsEditable)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        {
                            OnBeforeAnyAction();
                            MethodCallResult operationSuccessfull;
                            switch (_projectsSelectorMode)
                            {
                                case ProjectsSelectorModes.Rename:
                                    {
                                        //project about to be renamed
                                        operationSuccessfull = _projectsManager.RenameProject(SelectedProject, cbProjects.Text.Trim());
                                        if (operationSuccessfull)
                                        {
                                            btnRenameProject.IsChecked = false;
                                            SelectProject(SelectedProject.Name);
                                        }
                                        break;
                                    }
                                case ProjectsSelectorModes.Add:
                                    {
                                        //project about to be added
                                        Project createdProject;
                                        operationSuccessfull = _projectsManager.CreateProject(cbProjects.Text.Trim(), out createdProject);
                                        if (operationSuccessfull)
                                        {
                                            btnAddNewProject.IsChecked = false;
                                            Project[] projects;
                                            AssignSelectorItemsSource(out projects);
                                            SelectProject(createdProject.Name);
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        throw new ApplicationException("Invalid projects selection mode");
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
                            OnBeforeAnyAction();
                            btnAddNewProject.IsChecked = false;
                            btnRenameProject.IsChecked = false;
                            break;
                        }
                }
            }
        }
        private void SelectProject(string ProjectName)
        {
            cbProjects.SelectedItem = cbProjects.Items.Cast<Project>().FirstOrDefault(p => p.Name.Equals(ProjectName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
