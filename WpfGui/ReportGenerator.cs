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
            foreach (KeyValuePair<string, MonthWorktimeHistory[]> projectHistory in history.ByProjectsHistory)
            {
                Table projectTable = new Table();
                Document.Blocks.Add(projectTable);
                TableColumn[] columns = (new TableColumn[9]).Populate(new TableColumn());
                foreach (TableColumn column in columns)
                {
                    projectTable.Columns.Add(column);
                }
                TableRowGroup rowGroup = new TableRowGroup();
                projectTable.RowGroups.Add(rowGroup);

                //Add header row
                TableRow projectNameRow = new TableRow();
                rowGroup.Rows.Add(projectNameRow);
                TableCell projectNameCell = new TableCell(new Paragraph(new Run(projectHistory.Key)));
                projectNameRow.Cells.Add(projectNameCell);
                AddDaysOfWeekCells(projectNameRow);
                projectNameRow.Cells.Add(new TableCell(new Paragraph(new Run("Hours"))));

                Dictionary<DayOfWeek, TimeSpan> worktimeByDaysOfWeek = new Dictionary<DayOfWeek, TimeSpan>();
                foreach (MonthWorktimeHistory monthHistory in projectHistory.Value)
                {
                    for (int i = 0; i < monthHistory.WorktimeByWeeks.Length; i++)
                    {
                        WeekWorktimeHistory weekHistory = monthHistory.WorktimeByWeeks[i];

                        TableRow weekRow = new TableRow();
                        rowGroup.Rows.Add(weekRow);
                        TableCell weekNameCell = new TableCell(new Paragraph(new Run(monthHistory.Year + "-" + monthHistory.MonthOfTheYear + " Week " + i)));
                        weekRow.Cells.Add(weekNameCell);
                        
                        List<DayWorktimeHistory> worktimeByDays = weekHistory.WorktimeByDays.ToList();
                        foreach (DayOfWeek dayOfWeek in DaysOfWeekCollection.DaysOfWeek)
                        {
                            DayWorktimeHistory dayWorktime = worktimeByDays.FirstOrDefault(d => d.DayOfWeek == dayOfWeek);
                            TableCell dayWorktimeCell;
                            if (dayWorktime != null)
                            {
                                dayWorktimeCell = CreateDayWorktimeHistoryCell(dayWorktime);
                                worktimeByDaysOfWeek[dayOfWeek] += dayWorktime.TotalWorktime;
                            }
                            else
                            {
                                dayWorktimeCell = CreateNoWorktimeDayHistoryCell(dayWorktime.DayOfMonth);
                            }
                            weekRow.Cells.Add(dayWorktimeCell);
                        }
                        //Add ween total worktime cell in the end of week row
                        weekRow.Cells.Add(new TableCell(new Paragraph(new Run(weekHistory.TotalWorktime.ToString()))));
                    }
                }

                //Add 'total' row in the end of project table
                TableRow projectTotalRow = new TableRow();
                rowGroup.Rows.Add(projectTotalRow);
                projectTotalRow.Cells.Add(new TableCell());
                foreach (DayOfWeek dayOfWeek in DaysOfWeekCollection.DaysOfWeek)
                {
                    TimeSpan dayOfWeekWorktime;
                    if (worktimeByDaysOfWeek.TryGetValue(dayOfWeek, out dayOfWeekWorktime))
                    {
                        projectTotalRow.Cells.Add(new TableCell(new Paragraph(new Run(dayOfWeekWorktime.ToString()))));
                    }
                    else
                    {
                        projectTotalRow.Cells.Add(new TableCell());
                    }
                }
            }
            return MethodCallResult.Success;
        }
        private static TableCell CreateDayWorktimeHistoryCell(DayWorktimeHistory DayWorktimeHistory)
        {
            TableCell cell = new TableCell();
            Paragraph worktimeBlock = new Paragraph();
            worktimeBlock.TextAlignment = System.Windows.TextAlignment.Right;
            worktimeBlock.Inlines.Add(new Run(DayWorktimeHistory.TotalWorktime.ToString()));
            Paragraph dayBlock = new Paragraph(new Run(DayWorktimeHistory.DayOfMonth.ToString()));
            dayBlock.Typography.Variants = System.Windows.FontVariants.Superscript;
            dayBlock.TextAlignment = System.Windows.TextAlignment.Left;
            cell.Blocks.Add(worktimeBlock);
            cell.Blocks.Add(dayBlock);
            return cell;
        }
        private static TableCell CreateNoWorktimeDayHistoryCell(int DayOfMonth)
        {
            TableCell cell = new TableCell();
            Paragraph dayBlock = new Paragraph(new Run(DayOfMonth.ToString()));
            dayBlock.Typography.Variants = System.Windows.FontVariants.Superscript;
            dayBlock.TextAlignment = System.Windows.TextAlignment.Left;
            cell.Blocks.Add(dayBlock);
            return cell;
        }
        private void AddDaysOfWeekCells(TableRow Row)
        {
            foreach(DayOfWeek dayOfWeek in DaysOfWeekCollection.DaysOfWeek)
            {
                Row.Cells.Add(new TableCell(new Paragraph(new Run(dayOfWeek.ToString()))));
            }
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
