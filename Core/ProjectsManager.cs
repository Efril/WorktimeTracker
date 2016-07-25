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
    public class ProjectsManager: ManagerBase
    {
        #region -> Nested Fields <-
        
        private bool _projectsLoaded = false;
        private readonly Dictionary<string, Project> _projects = new Dictionary<string, Project>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region -> Interface <-

        public MethodCallResult GetAllProjects(out Project[] Projects)
        {
            MethodCallResult ensureProjectsLoadedResult= EnsureProjectsLoaded();
            if(!ensureProjectsLoadedResult)
            {
                Projects = new Project[0];
                return ensureProjectsLoadedResult;
            }
            else
            {
                Projects = _projects.Values.ToArray();
                return MethodCallResult.Success;
            }
        }

        public MethodCallResult DeleteProject(string ProjectName)
        {
            MethodCallResult projectsLoaded = EnsureProjectsLoaded();
            if (projectsLoaded)
            {
                Project projectToDelete;
                if (GetProject(ProjectName, out projectToDelete))
                {
                    MethodCallResult projectDeleted = TrackDb.DeleteProject(projectToDelete.ProjectId.Value);
                    return projectDeleted;
                }
                else
                {
                    return MethodCallResult.Success;
                }
            }
            else
            {
                return projectsLoaded;
            }
        }
        public MethodCallResult GetProject(string ProjectName, out Project Project)
        {
            MethodCallResult projectsLoaded = EnsureProjectsLoaded();
            if (projectsLoaded)
            {
                if(!_projects.TryGetValue(ProjectName, out Project))
                {
                    return MethodCallResult.CreateFail("Project `" + ProjectName + "` is unknown.");
                }
                else
                {
                    return MethodCallResult.Success;
                }
            }
            else
            {
                Project = null;
                return projectsLoaded;
            }
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
                MethodCallResult loadProjectsResult= TrackDb.GetAllProjects(out dbProjects);
                if(loadProjectsResult)
                {
                    foreach(DbProject dbProject in dbProjects)
                    {
                        _projects.Add(dbProject.Name, new Project(dbProject, TrackDb));
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
            MethodCallResult projectAdded= TrackDb.AddProject(ref dbProject);
            if (projectAdded) Project.BindToStorage(dbProject, TrackDb);
            return projectAdded;
        }

        #endregion
    }
}
