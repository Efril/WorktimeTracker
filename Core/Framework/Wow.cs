using Core.Framework.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework
{
    internal static class Wow
    {
        public static bool Is64BitProcess
        {
            get { return IntPtr.Size == 8; }
        }
        public static bool Is64BitOperatingSystem
        {
            get
            {
                // Clearly if this is a 64-bit process we must be on a 64-bit OS.
                if (Is64BitProcess)
                    return true;
                // Ok, so we are a 32-bit process, but is the OS 64-bit?
                // If we are running under Wow64 than the OS is 64-bit.
                bool isWow64;
                return ModuleContainsFunction("kernel32.dll", "IsWow64Process") && Kernel32.IsWow64Process(Kernel32.GetCurrentProcess(), out isWow64) && isWow64;
            }
        }

        static bool ModuleContainsFunction(string moduleName, string methodName)
        {
            IntPtr hModule = Kernel32.GetModuleHandle(moduleName);
            if (hModule != IntPtr.Zero)
                return Kernel32.GetProcAddress(hModule, methodName) != IntPtr.Zero;
            return false;
        }
    }
}
