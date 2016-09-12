using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework.Collections
{
    public class DaysOfWeekEnumerator : IEnumerator<DayOfWeek>
    {
        #region -> Nested Fields <-

        private readonly int _firstDayOfWeekCurrentCulture;
        private readonly int _enumFirstValue;
        private readonly int _enumLastValue;
        private DayOfWeek CurrentDayOfWeek
        {
            get;
            set;
        }
        private bool EnumerationStarted
        {
            get;
            set;
        }

        #endregion

        #region -> Interface <-

        public DayOfWeek Current
        {
            get { return CurrentDayOfWeek; }
        }
        object IEnumerator.Current
        {
            get { return this.Current; }
        }

        public void Dispose() { }

        public bool MoveNext()
        {
            if (EnumerationStarted)
            {
                int currentDayOfWeek = (int)this.CurrentDayOfWeek;
                currentDayOfWeek++;
                if (currentDayOfWeek > _enumLastValue) currentDayOfWeek = _enumFirstValue;
                this.CurrentDayOfWeek = (DayOfWeek)currentDayOfWeek;
                return (int)this.CurrentDayOfWeek != _firstDayOfWeekCurrentCulture;
            }
            else
            {
                EnumerationStarted = true;
                return true;
            }
        }
        public void Reset()
        {
            /*int currentDayOfWeek = _firstDayOfWeekCurrentCulture - 1;
            if (currentDayOfWeek < _enumFirstValue) currentDayOfWeek = _enumLastValue;*/

            this.CurrentDayOfWeek = (DayOfWeek)_firstDayOfWeekCurrentCulture;
            this.EnumerationStarted = false;
        }

        #endregion

        #region -> COnstructors <-

        public DaysOfWeekEnumerator()
            :this(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
        {
        }
        public DaysOfWeekEnumerator(DayOfWeek FirstDayOfWeek)
        {
            this._firstDayOfWeekCurrentCulture = (int)FirstDayOfWeek;
            int[] daysOfWeekEnum = (int[])Enum.GetValues(typeof(DayOfWeek));
            this._enumFirstValue = daysOfWeekEnum.First();
            this._enumLastValue = daysOfWeekEnum.Last();
            Reset();
        }

        #endregion
    }
}
