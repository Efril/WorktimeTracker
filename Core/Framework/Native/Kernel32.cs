using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework.Native
{
    internal static class Kernel32
    {
        ///
        /// Consts defined in WINBASE.H
        ///
        public enum MoveFileFlags
        {
            MOVEFILE_REPLACE_EXISTING = 1,
            MOVEFILE_COPY_ALLOWED = 2,
            MOVEFILE_DELAY_UNTIL_REBOOT = 4,
            MOVEFILE_WRITE_THROUGH = 8
        }

        public enum ShutdownPriorityLevels
        {
            FIRST_SHUTDOWN_RANGE = 0x3FF
        }

        public const int SHUTDOWN_NORETRY = 0x00000001;


        /// <summary>
        /// Marks the file for deletion during next system reboot
        /// </summary>
        /// <param name="lpExistingFileName">The current name of the file or directory on the local computer.</param>
        /// <param name="lpNewFileName">The new name of the file or directory on the local computer.</param>
        /// <param name="dwFlags">MoveFileFlags</param>
        /// <returns>bool</returns>
        /// <remarks>http://msdn.microsoft.com/en-us/library/aa365240(VS.85).aspx</remarks>
        [DllImport("kernel32.dll", EntryPoint = "MoveFileEx")]
        public static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);
        [DllImport("kernel32.dll")]
        public static extern bool SetProcessShutdownParameters(uint dwLevel, uint dwFlags);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool isWow64);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string moduleName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string methodName);
    }
}
