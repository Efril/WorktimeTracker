using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework
{
    internal static class RegistryHelper
    {
        #region -> Interface <-

        public static string RunKey
        {
            get { return Wow.Is64BitOperatingSystem ? SoftwareRegistryKey64 + @"Microsoft\Windows\CurrentVersion\Run" : SoftwareRegistryKey + @"Microsoft\Windows\CurrentVersion\Run"; }
        }
        public static string SoftwareRegistryKey
        {
            get { return @"SOFTWARE\"; }
        }
        public static string SoftwareRegistryKey64
        {
            get { return @"SOFTWARE\Wow6432Node\"; }
        }
        public static string UninstallRegistryKey64
        {
            get { return SoftwareRegistryKey64 + @"Microsoft\Windows\CurrentVersion\Uninstall"; }
        }
        public static string UninstallRegistryKey
        {
            get { return SoftwareRegistryKey + @"Microsoft\Windows\CurrentVersion\Uninstall"; }
        }
        public static string CurrentControlSetKey
        {
            get { return @"SYSTEM\CurrentControlSet\"; }
        }
        public static string CurentControlSetServicesKey
        {
            get { return CurrentControlSetKey + @"Services\"; }
        }
        public static string CurrentControlSetControlKey
        {
            get { return CurrentControlSetKey + @"Control\"; }
        }

        public static string[] GetLocalMachineKeyListValue(string Key, string ValueName)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(Key);
            return key != null ? key.GetValue(ValueName) as string[] : null;
        }
        public static string GetLocalMachineKeyStringValue(string Key, string ValueName)
        {
            /*using (RegistryKey localMachineKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {*/
            //Registry.LocalMachine.View = Wow.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            //using (RegistryKey key = localMachineKey.OpenSubKey("SOFTWARE\\Wow6432Node\\USNETXP\\Internet Visitors Savior for Windows - Cluster Edition\\"))//Registry.LocalMachine.OpenSubKey(Key);
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Key))
            {
                return key != null ? key.GetValue(ValueName) as string : null;
            }
            //}
        }
        public static void SetLocalMachineKeyValue(string Key, string ValueName, string Value)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(Key, true);
            if (key != null)
            {
                key.SetValue(ValueName, Value);
                key.Close();
            }
        }
        public static void SetLocalMachineKeyValue(string Key, string ValueName, string[] Value)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(Key, true);
            if (key != null)
            {
                key.SetValue(ValueName, Value, RegistryValueKind.MultiString);
                key.Close();
            }
        }
        public static void EnsureLocalMachineKeyExist(string Key)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(Key, true);
            if (key == null) Registry.LocalMachine.CreateSubKey(Key);
        }
        public static string GetSuitableUninstallRegistryKey()
        {
            return Wow.Is64BitOperatingSystem ? UninstallRegistryKey64 : UninstallRegistryKey;
        }
        public static bool CheckLocalMachineKeyExist(string Key)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(Key);
            bool result = false;
            if (key != null)
            {
                result = true;
                key.Close();
            }
            return result;
        }
        public static RegistryKey OpenServiceRegistryKey(string Host, string ServiceName, bool OpenWritable)
        {
            Host = FixHostForRegistry(Host);
            RegistryKey localMachine = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, Host);
            RegistryKey servicesKey = localMachine.OpenSubKey(CurentControlSetServicesKey);
            try
            {
                foreach (string serviceName in servicesKey.GetSubKeyNames())
                {
                    if (serviceName.Equals(ServiceName, StringComparison.OrdinalIgnoreCase))
                    {
                        return servicesKey.OpenSubKey(serviceName, OpenWritable);
                    }
                }
                return null;
            }
            finally
            {
                servicesKey.Close();
                localMachine.Close();
            }
        }
        public static RegistryKey OpenServiceRegistryKey(string ServiceName, bool OpenWritable)
        {
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            RegistryKey servicesKey = localMachine.OpenSubKey(CurentControlSetServicesKey);
            try
            {
                foreach (string serviceName in servicesKey.GetSubKeyNames())
                {
                    if (serviceName.Equals(ServiceName, StringComparison.OrdinalIgnoreCase))
                    {
                        return servicesKey.OpenSubKey(serviceName, OpenWritable);
                    }
                }
                return null;
            }
            finally
            {
                servicesKey.Close();
                localMachine.Close();
            }
        }
        public static string FindServiceDisplayName(string Host, string ServiceName)
        {
            string result = null;
            RegistryKey serviceKey = OpenServiceRegistryKey(Host, ServiceName, false);
            if (serviceKey != null)
            {
                result = serviceKey.GetValue("DisplayName").ToString();
                serviceKey.Close();
            }
            return result;
        }
        public static string FindInstallLocationByProductCode(string Host, string ProductCode)
        {
            string result = FindInstallLocationByProductCode(Host, ProductCode, UninstallRegistryKey, RegistryView.Registry64);
            if (result == null) result = FindInstallLocationByProductCode(Host, ProductCode, UninstallRegistryKey64, RegistryView.Registry32);
            if (result != null) result = result.TrimEnd('\\');
            return result;
        }
        public static string FindInstallLocationByDisplayName(string Host, string DisplayName)
        {
            string result = FindInstallLocationByDisplayName(Host, DisplayName, UninstallRegistryKey, RegistryView.Registry64);
            if (result == null) result = FindInstallLocationByDisplayName(Host, DisplayName, UninstallRegistryKey64, RegistryView.Registry32);
            if (result != null) result = result.TrimEnd('\\');
            return result;
        }

        #endregion

        #region -> Nested Methods <-

        private static string FindInstallLocationByProductCode(string Host, string ProductCode, string UninstallRegistryKey, RegistryView View)
        {
            Host = FixHostForRegistry(Host);
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, View);
            string result = null;
            RegistryKey uninstallKey = localMachine.OpenSubKey(UninstallRegistryKey);
            if (uninstallKey != null)
            {
                foreach (string appName in uninstallKey.GetSubKeyNames())
                {
                    if (appName.Equals(ProductCode, StringComparison.OrdinalIgnoreCase))
                    {
                        RegistryKey appKey = uninstallKey.OpenSubKey(appName);
                        result = appKey.GetValue("InstallLocation").ToString();
                        appKey.Close();
                        break;
                    }
                }
                uninstallKey.Close();
            }
            localMachine.Close();
            return result;
        }
        private static string FindInstallLocationByDisplayName(string Host, string DisplayName, string UninstallRegistryKey, RegistryView View)
        {
            Host = FixHostForRegistry(Host);
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, View);
            string result = null;
            RegistryKey uninstallKey = localMachine.OpenSubKey(UninstallRegistryKey);
            if (uninstallKey != null)
            {
                foreach (string appName in uninstallKey.GetSubKeyNames())
                {
                    RegistryKey appKey = uninstallKey.OpenSubKey(appName);
                    object displayName = appKey.GetValue("DisplayName");
                    if (displayName != null && (displayName as string).StartsWith(DisplayName, StringComparison.OrdinalIgnoreCase))
                    {
                        result = appKey.GetValue("InstallLocation").ToString();
                        appKey.Close();
                        break;
                    }
                    appKey.Close();
                }
                uninstallKey.Close();
            }
            localMachine.Close();
            return result;
        }
        private static string FixHostForRegistry(string Host)
        {
            Host = string.IsNullOrEmpty(Host) ? "localhost" : Host;
            if (Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || Host.StartsWith("127.0.0", StringComparison.OrdinalIgnoreCase))
            {
                Host = ApplicationServices.GetFQDN();
            }
            return Host;
        }

        #endregion
    }
}
