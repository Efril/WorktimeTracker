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
                return new MonthWorktimeHistory(Year, MonthOfTheYear, WorktimeByWeeks.Select(w => w.ToWeekWorktimeHistory()).ToArray(), WorktimeByDays.Select(d => d.ToDayWorktimeHistory()).ToArray(), new ReadOnlyDictionary<string, TimeSpan>(ByProjectsTotalWorktime));
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
                return new WeekWorktimeHistory(WeekOfTheYear, WorktimeByDays.Select(d => d.ToDayWorktimeHistory()).ToArray(), new ReadOnlyDictionary<string, TimeSpan>(ByProjectsTotalWorktime));
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
                return new DayWorktimeHistory(DayOfWeek, DayOfMonth, new ReadOnlyDictionary<string, TimeSpan>(ByProjectsTotalWorktime));
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

                    //Update allProjectsHistory
                    EditableMonthWorktimeHistory allProjectsMonthHistory;
                    EditableWeekWorktimeHistory allProjectsWeekHistory;
                    EditableDayWorktimeHistory allProjectsDayHistory;
                    GetTimeIntervalHistoryContainers(allProjectsHistory, historyDate, weekNumber, out allProjectsMonthHistory, out allProjectsWeekHistory, out allProjectsDayHistory);

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

                        //Update byProjectsHistory
                        List<EditableMonthWorktimeHistory> projectByMonthsHistory;
                        if(!byProjectsHistory.TryGetValue(projectName, out projectByMonthsHistory))
                        {
                            projectByMonthsHistory = new List<EditableMonthWorktimeHistory>();
                            byProjectsHistory.Add(projectName, projectByMonthsHistory);
                        }
                        EditableMonthWorktimeHistory projectMonth;
                        EditableWeekWorktimeHistory projectWeek;
                        EditableDayWorktimeHistory projectDay;
                        GetTimeIntervalHistoryContainers(projectByMonthsHistory, historyDate, weekNumber, out projectMonth, out projectWeek, out projectDay);
                        projectDay.ByProjectsTotalWorktime[projectName] = elapsedWorktime;
                        projectWeek.ByProjectsTotalWorktime[projectName] += elapsedWorktime;
                        projectMonth.ByProjectsTotalWorktime[projectName] += elapsedWorktime;

                        allProjectsMonthHistory.ByProjectsTotalWorktime[projectName] += elapsedWorktime;
                        allProjectsWeekHistory.ByProjectsTotalWorktime[projectName] += elapsedWorktime;
                        allProjectsDayHistory.ByProjectsTotalWorktime[projectName] += elapsedWorktime;
                    }
                    historyDate.AddDays(1);
                }
                History = new History(allProjectsHistory.Select(m => m.ToMonthWorktimeHistory()).ToArray(), new ReadOnlyDictionary<string, MonthWorktimeHistory[]>(byProjectsHistory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(m => m.ToMonthWorktimeHistory()).ToArray())), new ReadOnlyDictionary<string, TimeSpan>(byProjectsTotalWorktime));
                return MethodCallResult.Success;
            }
            catch(Exception ex)
            {
                History = null;
                return MethodCallResult.CreateException(ex);
            }
        }
        private void GetTimeIntervalHistoryContainers(List<EditableMonthWorktimeHistory> MonthHistoryContainers, DateTime HistoryDate, int HistoryDateWeekOfTheYear, out EditableMonthWorktimeHistory MonthHistoryContainer, out EditableWeekWorktimeHistory WeekHistoryContainer, out EditableDayWorktimeHistory DayHistoryContainer)
        {
            MonthHistoryContainer = MonthHistoryContainers.FirstOrDefault(m => m.Year == HistoryDate.Year && m.MonthOfTheYear == HistoryDate.Month);
            if (MonthHistoryContainer == null)
            {
                MonthHistoryContainer = new EditableMonthWorktimeHistory(HistoryDate.Year, HistoryDate.Month);
                MonthHistoryContainers.Add(MonthHistoryContainer);
                WeekHistoryContainer = new EditableWeekWorktimeHistory(HistoryDateWeekOfTheYear);
                MonthHistoryContainer.WorktimeByWeeks.Add(WeekHistoryContainer);
                DayHistoryContainer = new EditableDayWorktimeHistory(HistoryDate.Day, HistoryDate.DayOfWeek);
                WeekHistoryContainer.WorktimeByDays.Add(DayHistoryContainer);
                MonthHistoryContainer.WorktimeByDays.Add(DayHistoryContainer);
            }
            else
            {
                WeekHistoryContainer = MonthHistoryContainer.WorktimeByWeeks.FirstOrDefault(w => w.WeekOfTheYear == HistoryDateWeekOfTheYear);
                if (WeekHistoryContainer == null)
                {
                    WeekHistoryContainer = new EditableWeekWorktimeHistory(HistoryDateWeekOfTheYear);
                    MonthHistoryContainer.WorktimeByWeeks.Add(WeekHistoryContainer);
                    DayHistoryContainer = new EditableDayWorktimeHistory(HistoryDate.Day, HistoryDate.DayOfWeek);
                    WeekHistoryContainer.WorktimeByDays.Add(DayHistoryContainer);
                    MonthHistoryContainer.WorktimeByDays.Add(DayHistoryContainer);
                }
                else
                {
                    DayHistoryContainer = WeekHistoryContainer.WorktimeByDays.FirstOrDefault(d => d.DayOfMonth == HistoryDate.Day);
                    if (DayHistoryContainer == null)
                    {
                        DayHistoryContainer = new EditableDayWorktimeHistory(HistoryDate.Day, HistoryDate.DayOfWeek);
                        WeekHistoryContainer.WorktimeByDays.Add(DayHistoryContainer);
                        MonthHistoryContainer.WorktimeByDays.Add(DayHistoryContainer);
                    }
                }
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
