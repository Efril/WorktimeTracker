using Core.Framework;
using Core.Storage;
using Core.Storage.DbTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ProjectsManager
    {
        #region -> Nested Fields <-

        private const string _trackDbFileName = "trackdb.sqlite";
        private TrackDbProvider _trackDb;
        private bool _projectsLoaded = false;
        private readonly Dictionary<string, Project> _projects = new Dictionary<string, Project>(StringComparer.OrdinalIgnoreCase);

        #endregion

        public Project[] Projects
        {
            get { return _projects.Values.ToArray(); }
        }

        public MethodCallResult CreateProject(string ProjectName, out Project Project)
        {
            MethodCallResult projectsLoaded = EnsureProjectsLoaded();
            if (projectsLoaded)
            {
                if (_projects.ContainsKey(ProjectName))
                {
                    Project = null;
                    return MethodCallResult.CreateFail("Project '" + ProjectName + "' already exist.");
                }
                else
                {
                    Project = new Project(ProjectName);
                    MethodCallResult addedToStorage = AddProjectToStorage(Project);
                    if(addedToStorage)
                    {
                        _projects.Add(Project.Name, Project);
                        return MethodCallResult.Success;
                    }
                    else
                    {
                        Project = null;
                        return addedToStorage;
                    }
                }
            }
            else
            {
                Project = null;
                return projectsLoaded;
            }
        }
        private MethodCallResult EnsureProjectsLoaded()
        {
            if(!_projectsLoaded)
            {
                _projects.Clear();
                DbProject[] dbProjects;
                MethodCallResult loadProjectsResult= _trackDb.GetAllProjects(out dbProjects);
                if(loadProjectsResult)
                {
                    foreach(DbProject dbProject in dbProjects)
                    {
                        _projects.Add(dbProject.Name, new Project(dbProject, _trackDb));
                    }
                }
                return loadProjectsResult;
            }
            else
            {
                return MethodCallResult.Success;
            }
        }
        private MethodCallResult AddProjectToStorage(Project Project)
        {
            DbProject dbProject = new DbProject(null, Project.Name);
            MethodCallResult projectAdded= _trackDb.AddProject(ref dbProject);
            if (projectAdded) Project.BindToStorage(dbProject, _trackDb);
            return projectAdded;
        }

        public ProjectsManager()
        {
            _trackDb = new TrackDbProvider(new SqliteDatabaseConnectionString(Path.Combine(ApplicationServices.GetAssemblyFolderPath(), _trackDbFileName)));
        }
    }
}
