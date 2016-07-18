using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Core.Framework
{
    internal class Timer
    {
        #region -> Nested Fields <-

        private readonly System.Timers.Timer _timer;
        private volatile bool _running = false;

        #endregion

        #region -> Interface <-

        public event ElapsedEventHandler Elapsed;

        public double Interval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        public void Start()
        {
            _running = true;
            _timer.Start();
        }
        public void Stop()
        {
            _running = false;
            _timer.Stop();
        }

        #endregion

        public Timer(double IntervalMs)
        {
            _timer = new System.Timers.Timer(IntervalMs);
            _timer.AutoReset = false;
            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Elapsed != null) Elapsed(this, e);
            if (_running) _timer.Enabled = true;
        }
    }
}
