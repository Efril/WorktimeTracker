using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework
{
    internal static class ApplicationServices
    {
        private static readonly Dictionary<string, NamedPipeServerStream> _singletonPipes = new Dictionary<string, NamedPipeServerStream>();

        public static string GetFQDN()
        {
            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();
            string fqdn = "";
            if (!hostName.Contains(domainName)) fqdn = hostName + "." + domainName;
            else fqdn = hostName;
            return fqdn;
        }

        public static string GetAssemblyFolderPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        /// <summary>
        /// Also creates singleton pipe with given name if it was not exist
        /// </summary>
        /// <param name="PipeName"></param>
        /// <returns></returns>
        public static bool CheckApplicationStarted(string PipeName)
        {
            lock (_singletonPipes)
            {
                NamedPipeClientStream singletonPipeClient = new NamedPipeClientStream(".", PipeName, PipeAccessRights.FullControl, PipeOptions.WriteThrough, System.Security.Principal.TokenImpersonationLevel.None, HandleInheritability.None);
                bool alreadyStarted = true;
                try
                {
                    singletonPipeClient.Connect(200);
                    singletonPipeClient.Close();
                }
                catch
                {
                    alreadyStarted = false;
                    CreateSingletonPipe(PipeName);
                }
                return alreadyStarted;
            }
        }
        public static void CloseSingletonPipe(string PipeName)
        {
            lock (_singletonPipes)
            {
                if (_singletonPipes.ContainsKey(PipeName))
                {
                    _singletonPipes[PipeName].Close();
                    _singletonPipes.Remove(PipeName);
                }
            }
        }
        private static void CreateSingletonPipe(string PipeName)
        {
            if (!_singletonPipes.ContainsKey(PipeName)) _singletonPipes.Add(PipeName, new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1));
        }

        #region -> Application autorun logic <-

        public static MethodCallResult TryGetApplicationPinnedToAutorun(string ApplicationPath, out bool PinnedToAutorun)
        {
            string appKeyName = Path.GetFileNameWithoutExtension(ApplicationPath);
            try
            {
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(RegistryHelper.RunKey, false))
                {
                    PinnedToAutorun = registryKey.GetValue(appKeyName) != null;
                }
                return MethodCallResult.Success;
            }
            catch(Exception ex)
            {
                PinnedToAutorun = false;
                return MethodCallResult.CreateException(ex);
            }
        }

        public static MethodCallResult PinToAutorun(string ApplicationPath)
        {
            string appKeyName = Path.GetFileNameWithoutExtension(ApplicationPath);
            try
            {
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(RegistryHelper.RunKey, true))
                {
                    if (registryKey.GetValue(appKeyName) == null)
                    {
                        //System.Windows.Forms.MessageBox.Show("Pin1");
                        registryKey.SetValue(appKeyName, ApplicationPath);
                    }
                }
                return MethodCallResult.Success;
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(TextLogProvider.CreateExceptionData(ex));
                return MethodCallResult.CreateException(ex);
            }
        }
        public static MethodCallResult UnpinFromAutorun(string ApplicationPath)
        {
            string appKeyName = Path.GetFileNameWithoutExtension(ApplicationPath);
            try
            {
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(RegistryHelper.RunKey, true))
                {
                    if (registryKey.GetValue(appKeyName) != null)
                    {
                        //System.Windows.Forms.MessageBox.Show("Unpin1");
                        registryKey.DeleteValue(appKeyName);
                    }
                }
                return MethodCallResult.Success;
            }
            catch (Exception ex)
            {
                return MethodCallResult.CreateException(ex);
            }
        }

        #endregion
    }
}
