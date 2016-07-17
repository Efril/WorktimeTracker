using Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ProjectsManager
    {
        private readonly Dictionary<string, Project> _projects = new Dictionary<string, Project>(StringComparer.OrdinalIgnoreCase);

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

        }
        private MethodCallResult AddProjectToStorage(Project Project)
        {

        }
    }
}
