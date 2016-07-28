using Core.Framework.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework
{
    public class LayoutManager
    {
        public static void GetAboveTrayNotificationAreaWindowPosition(double WindowWidth, double WindowHeight, double ScreenWidth, double ScreenHeight, out double Top, out double Left)
        {
            //Get taskbar size and position
            ShellApi.ABE taskbarEdge;
            int taskbarHeight;
            bool taskbarAutoHide;
            ShellApi.GetTaskBarInfo(out taskbarEdge, out taskbarHeight, out taskbarAutoHide);

            int topFixBecauseOfTaskbar = 0;
            int leftFixBecauseOfTaskbar = 0;
            switch(taskbarEdge)
            {
                case ShellApi.ABE._BOTTOM:
                    {
                        topFixBecauseOfTaskbar = taskbarHeight;
                        break;
                    }
                case ShellApi.ABE._RIGHT:
                    {
                        leftFixBecauseOfTaskbar = taskbarHeight;
                        break;
                    }
            }
            Top = ScreenHeight - WindowHeight - topFixBecauseOfTaskbar;
            Left = ScreenWidth - WindowWidth - leftFixBecauseOfTaskbar;
        }
    }
}
