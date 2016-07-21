using Core.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class HistoryProvider:ManagerBase
    {
        #region -> Types <-

        class EditableHistoryBase
        {
            public readonly Dictionary<string, TimeSpan> ByProjectsTotalWorktime = new Dictionary<string, TimeSpan>();
            public TimeSpan CalculateTotalWorktime()
            {
                return TimeSpan.FromSeconds(ByProjectsTotalWorktime.Values.Sum(s => s.TotalSeconds));
            }
        }
        class EditableMonthWorktimeHistory: EditableHistoryBase
        {
            private readonly int _hash;

            public readonly int Year;
            public readonly int MonthOfTheYear;
            public readonly List<EditableWeekWorktimeHistory> WorktimeByWeeks = new List<EditableWeekWorktimeHistory>(4);
            public readonly List<EditableDayWorktimeHistory> WorktimeByDays = new List<EditableDayWorktimeHistory>();

            public override bool Equals(object obj)
            {
                EditableMonthWorktimeHistory other = obj as EditableMonthWorktimeHistory;
                return other != null && other.Year == this.Year && other.MonthOfTheYear == this.MonthOfTheYear;
            }
            public override int GetHashCode()
            {
                return _hash;
            }

            public MonthWorktimeHistory ToMonthWorktimeHistory()
            {
                return new MonthWorktimeHistory(Year, MonthOfTheYear, WorktimeByWeeks.Select(w => w.ToWeekWorktimeHistory()).ToArray(), WorktimeByDays.Select(d => d.ToDayWorktimeHistory()).ToArray(), new ReadOnlyDictionary<string, TimeSpan>(ByProjectsTotalWorktime), CalculateTotalWorktime());
            }

            public EditableMonthWorktimeHistory(int Year, int MonthOfTheYear)
            {
                this.Year = Year;
                this.MonthOfTheYear = MonthOfTheYear;
                _hash = HashCalculator.Calculate(Year, MonthOfTheYear);
            }
        }
        class EditableWeekWorktimeHistory: EditableHistoryBase
        {
            public readonly int WeekOfTheYear;

            public readonly HashSet<EditableDayWorktimeHistory> WorktimeByDays = new HashSet<EditableDayWorktimeHistory>();

            public override bool Equals(object obj)
            {
                EditableWeekWorktimeHistory other = obj as EditableWeekWorktimeHistory;
                return other != null && other.WeekOfTheYear == this.WeekOfTheYear;
            }
            public override int GetHashCode()
            {
                return WeekOfTheYear.GetHashCode();
            }

            public WeekWorktimeHistory ToWeekWorktimeHistory()
            {
                return new WeekWorktimeHistory(WeekOfTheYear, WorktimeByDays.Select(d => d.ToDayWorktimeHistory()).ToArray(), new ReadOnlyDictionary<string, TimeSpan>(ByProjectsTotalWorktime), CalculateTotalWorktime());
            }

            public EditableWeekWorktimeHistory(int WeekOfTheYear)
            {
                this.WeekOfTheYear = WeekOfTheYear;
            }
        }
        class EditableDayWorktimeHistory : EditableHistoryBase
        {
            private readonly int _hash;

            public readonly int DayOfMonth;
            public readonly DayOfWeek DayOfWeek;

            public override bool Equals(object obj)
            {
                EditableDayWorktimeHistory other = obj as EditableDayWorktimeHistory;
                return other != null && other.DayOfMonth == this.DayOfMonth && other.DayOfWeek == this.DayOfWeek;
            }
            public override int GetHashCode()
            {
                return _hash;
            }

            public DayWorktimeHistory ToDayWorktimeHistory()
            {
                return new DayWorktimeHistory(DayOfWeek, DayOfMonth, new ReadOnlyDictionary<string, TimeSpan>(ByProjectsTotalWorktime), CalculateTotalWorktime());
            }

            public EditableDayWorktimeHistory(int DayOfMonth, DayOfWeek DayOfWeek)
            {
                this.DayOfMonth = DayOfMonth;
                this.DayOfWeek = DayOfWeek;
                _hash = HashCalculator.Calculate(DayOfMonth, (int)DayOfWeek);
            }
        }

        #endregion

        public MethodCallResult GetHistory(ProjectsManager ProjectsManager, string[] Projects, DateTime From, DateTime Till, out History History)
        {
            try
            {
                Dictionary<string, List<EditableMonthWorktimeHistory>> byProjectsHistory = new Dictionary<string, List<EditableMonthWorktimeHistory>>(Projects.Length);
                Dictionary<string, TimeSpan> byProjectsTotalWorktime = new Dictionary<string, TimeSpan>(Projects.Length);
                List<EditableMonthWorktimeHistory> allProjectsHistory = new List<EditableMonthWorktimeHistory>();
                DateTime historyDate = From.Date;
                while (historyDate <= Till)
                {
                    int weekNumber = historyDate.GetIso8601WeekOfYear();
                    foreach (string projectName in Projects)
                    {
                        TimeSpan elapsedWorktime;
                        MethodCallResult elapsedWorktimeFetched = GetProjectWorktimeElapsed(projectName, historyDate, ProjectsManager, out elapsedWorktime);
                        if(!elapsedWorktimeFetched)
                        {
                            History = null;
                            return elapsedWorktimeFetched;
                        }

                        byProjectsTotalWorktime[projectName] += elapsedWorktime;

                        List<EditableMonthWorktimeHistory> projectByMonthsHistory;
                        if(!byProjectsHistory.TryGetValue(projectName, out projectByMonthsHistory))
                        {
                            projectByMonthsHistory = new List<EditableMonthWorktimeHistory>();
                            byProjectsHistory.Add(projectName, projectByMonthsHistory);
                        }
                        EditableMonthWorktimeHistory projectMonth = projectByMonthsHistory.FirstOrDefault(m => m.Year == historyDate.Year && m.MonthOfTheYear == historyDate.Month);
                        EditableWeekWorktimeHistory projectWeek;
                        EditableDayWorktimeHistory projectDay;
                        if (projectMonth == null)
                        {
                            projectMonth = new EditableMonthWorktimeHistory(historyDate.Year, historyDate.Month);
                            projectByMonthsHistory.Add(projectMonth);
                            projectWeek = new EditableWeekWorktimeHistory(weekNumber);
                            projectMonth.WorktimeByWeeks.Add(projectWeek);
                            projectDay = new EditableDayWorktimeHistory(historyDate.Day, historyDate.DayOfWeek);
                            projectWeek.WorktimeByDays.Add(projectDay);
                        }
                        else
                        {
                            projectWeek = projectMonth.WorktimeByWeeks.FirstOrDefault(w => w.WeekOfTheYear == weekNumber);
                            if (projectWeek == null)
                            {
                                projectWeek = new EditableWeekWorktimeHistory(weekNumber);
                                projectMonth.WorktimeByWeeks.Add(projectWeek);
                                projectDay = new EditableDayWorktimeHistory(historyDate.Day, historyDate.DayOfWeek);
                                projectWeek.WorktimeByDays.Add(projectDay);
                            }
                            else
                            {
                                projectDay = projectWeek.WorktimeByDays.FirstOrDefault(d => d.DayOfMonth == historyDate.Day);
                                if(projectDay==null)
                                {
                                    projectDay = new EditableDayWorktimeHistory(historyDate.Day, historyDate.DayOfWeek);
                                    projectWeek.WorktimeByDays.Add(projectDay);
                                }
                            }
                        }
                        projectDay.ByProjectsTotalWorktime[projectName] = elapsedWorktime;
                        projectWeek.ByProjectsTotalWorktime[projectName] += elapsedWorktime;
                        projectMonth.ByProjectsTotalWorktime[projectName] += elapsedWorktime;

                        allProjectsHistory!!!!!!
                    }
                    historyDate.AddDays(1);
                }
            }
            catch(Exception ex)
            {
                History = null;
                return MethodCallResult.CreateException(ex);
            }
        }
        private MethodCallResult GetProjectWorktimeElapsed(string ProjectName, DateTime Date, ProjectsManager ProjectsManager, out TimeSpan ElapsedWorktime)
        {
            Project project;
            MethodCallResult projectFound = ProjectsManager.GetProject(ProjectName, out project);
            if (!projectFound)
            {
                ElapsedWorktime = TimeSpan.MinValue;
                return projectFound;
            }
            return TrackDb.GetElapsedWorktime(project.ProjectId.Value, Date, out ElapsedWorktime);
        }
    }
}
