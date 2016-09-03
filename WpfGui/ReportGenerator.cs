using Core;
using Core.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Core.Framework.Collections;
using System.Globalization;

namespace WpfGui
{
    internal class ReportGenerator
    {
        private readonly ReportWindowProjectItem[] ProjectItems;
        private readonly DateTime DateFrom;
        private readonly DateTime DateTo;
        private readonly HistoryProvider HistoryProvider;

        public MethodCallResult TryGenerate(out FlowDocument Document)
        {
            History history;
            MethodCallResult result = HistoryProvider.GetHistory(ProjectItems.Select(p => p.ProjectName).ToArray(), DateFrom, DateTo, out history);
            if (!result)
            {
                Document = null;
                return result;
            }
            Document = new FlowDocument();
            foreach(KeyValuePair<string, MonthWorktimeHistory[]> projectHistory in history.ByProjectsHistory)
            {
                Table projectTable = new Table();
                TableColumn[] columns = (new TableColumn[9]).Populate(new TableColumn());
                foreach (TableColumn column in columns)
                {
                    projectTable.Columns.Add(column);
                }
                TableRowGroup rowGroup = new TableRowGroup();

                //Add header row
                TableRow projectNameRow = new TableRow();
                TableCell projectNameCell = new TableCell(new Paragraph(new Run(projectHistory.Key)));
                projectNameRow.Cells.Add(projectNameCell);
                AddDaysOfWeekCells(projectNameRow);
                projectNameRow.Cells.Add(new TableCell(new Paragraph(new Run("Hours"))));

                foreach(MonthWorktimeHistory monthHistory in projectHistory.Value)
                {
                   for(int i=0;i<monthHistory.WorktimeByWeeks.Length;i++)
                    {
                        WeekWorktimeHistory weekHistory = monthHistory.WorktimeByWeeks[i];

                        TableRow weekRow = new TableRow();
                        TableCell weekNameCell = new TableCell(new Paragraph(new Run(monthHistory.Year + "-" + monthHistory.MonthOfTheYear + " Week " + i)));
                        weekRow.Cells.Add(weekNameCell);

                        int totalWeekHours = 0;
                        List<DayWorktimeHistory> worktimeByDays = weekHistory.WorktimeByDays.ToList();
                        for (int j = 0; j < 7; j++)
                        {
                            DayWorktimeHistory dayWorktime= worktimeByDays.FirstOrDefault(d=>d.DayOfWeek)
                        }

                        foreach(DayWorktimeHistory dayHistory in weekHistory.WorktimeByDays)
                        {
                            TableCell dayCell = CreateDayWorktimeHistoryCell(dayHistory);
                            weekRow.Cells.Add(dayCell);
                        }
                    }
                }
            }
        }
        private static TableCell CreateDayWorktimeHistoryCell(DayWorktimeHistory DayWorktimeHistory)
        {

        }
        private static TableCell CreateNoWorktimeDayHistoryCell(int DayOfMonth)
        {

        }
        private void AddDaysOfWeekCells(TableRow Row)
        {
            int firstDayOfWeek = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            int currentDayOfWeek = firstDayOfWeek;
            int[] daysOfWeekEnum = (int[])Enum.GetValues(typeof(DayOfWeek));
            int firstDayOfWeekInEnum = daysOfWeekEnum.First();
            int lastDaysOfWeekInEnum = daysOfWeekEnum.Last();
            do
            {
                Row.Cells.Add(new TableCell(new Paragraph(new Run(((DayOfWeek)currentDayOfWeek).ToString()))));
                currentDayOfWeek++;
                if (currentDayOfWeek > lastDaysOfWeekInEnum) currentDayOfWeek = firstDayOfWeekInEnum;
            }
            while (currentDayOfWeek != firstDayOfWeek);
        }

        public ReportGenerator(ReportWindowProjectItem[] ProjectItems, DateTime DateFrom, DateTime DateTo, HistoryProvider HistoryProvider)
        {
            Contract.Requires(ProjectItems != null);
            Contract.Requires(HistoryProvider != null);
            this.HistoryProvider = HistoryProvider;
            this.ProjectItems = ProjectItems;
            this.DateFrom = DateFrom;
            this.DateTo = DateTo;
        }
    }
}
