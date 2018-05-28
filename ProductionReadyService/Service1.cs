using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Win32;

namespace ProductionReadyService
{
    public partial class Service1 : ServiceBase
    {
       
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Thread trd = new Thread(runner);
            trd.IsBackground = true;
            trd.Start();
        }

        protected override void OnStop()
        {
            EnableUpdateSvc();
        }

        protected void runner()
        {

            do
            {
                readSettings();
                if (DisableUpdates)
                {
                    if (pmode)
                    {
                        if (Days.Contains(((int)DateTime.Now.DayOfWeek).ToString()))
                        {
                            DisableUpdateSvc();
                        }
                        else
                        {
                            EnableUpdateSvc();
                        }
                    }
                    else
                    {
                        EnableUpdateSvc();
                    }
                }else
                {
                    EnableUpdateSvc();
                }
                
                // Sleep for 1 hour
                Thread.Sleep(3600000);
            } while (1 == 1);
        }

        public bool pmode = false;
        public bool DisableUpdates = false;
        public string Days = "";

        protected void readSettings()
        {
            RegistryKey root = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Semrau Software Consulting").OpenSubKey("ProductionReady");

            pmode = bool.Parse(root.GetValue("pmode", "False").ToString());

            DisableUpdates = bool.Parse(root.GetValue("DisableUpdates", "False").ToString());

            Days = root.GetValue("ProductionDays", "").ToString();
            
            if (Days.Split(',').Count() > 3)
            {
                pmode = false;
            }
        }

        protected void DisableUpdateSvc()
        {
            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\wuauserv", "ImagePath", null) != null)
            {
                Process proc = new Process();
                proc.StartInfo.FileName = @"C:\Windows\System32\net.exe";
                proc.StartInfo.Arguments = @"stop wuauserv";
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
                RenameSubKey(Registry.LocalMachine.OpenSubKey("SYSTEM").OpenSubKey("ControlSet001").OpenSubKey("Services", true), "wuauserv", "wuauserv-prod");
            }
        }
        protected void EnableUpdateSvc()
        {
            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\wuauserv-prod", "ImagePath", null) != null)
            {
                RenameSubKey(Registry.LocalMachine.OpenSubKey("SYSTEM").OpenSubKey("ControlSet001").OpenSubKey("Services", true), "wuauserv-prod", "wuauserv");
                Process proc = new Process();
                proc.StartInfo.FileName = @"C:\Windows\System32\net.exe";
                proc.StartInfo.Arguments = @"start wuauserv";
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
            }
        }



        /// <summary>
        /// Renames a subkey of the passed in registry key since 
        /// the Framework totally forgot to include such a handy feature.
        /// </summary>
        /// <param name="regKey">The RegistryKey that contains the subkey 
        /// you want to rename (must be writeable)</param>
        /// <param name="subKeyName">The name of the subkey that you want to rename
        /// </param>
        /// <param name="newSubKeyName">The new name of the RegistryKey</param>
        /// <returns>True if succeeds</returns>
        public bool RenameSubKey(RegistryKey parentKey,
            string subKeyName, string newSubKeyName)
        {
            CopyKey(parentKey, subKeyName, newSubKeyName);
            parentKey.DeleteSubKeyTree(subKeyName);
            return true;
        }

        /// <summary>
        /// Copy a registry key.  The parentKey must be writeable.
        /// </summary>
        /// <param name="parentKey"></param>
        /// <param name="keyNameToCopy"></param>
        /// <param name="newKeyName"></param>
        /// <returns></returns>
        public bool CopyKey(RegistryKey parentKey,
            string keyNameToCopy, string newKeyName)
        {
            //Create new key
            RegistryKey destinationKey = parentKey.CreateSubKey(newKeyName);

            //Open the sourceKey we are copying from
            RegistryKey sourceKey = parentKey.OpenSubKey(keyNameToCopy);

            RecurseCopyKey(sourceKey, destinationKey);

            return true;
        }

        private void RecurseCopyKey(RegistryKey sourceKey, RegistryKey destinationKey)
        {
            //copy all the values
            foreach (string valueName in sourceKey.GetValueNames())
            {
                object objValue = sourceKey.GetValue(valueName);
                RegistryValueKind valKind = sourceKey.GetValueKind(valueName);
                destinationKey.SetValue(valueName, objValue, valKind);
            }

            //For Each subKey 
            //Create a new subKey in destinationKey 
            //Call myself 
            foreach (string sourceSubKeyName in sourceKey.GetSubKeyNames())
            {
                RegistryKey sourceSubKey = sourceKey.OpenSubKey(sourceSubKeyName);
                RegistryKey destSubKey = destinationKey.CreateSubKey(sourceSubKeyName);
                RecurseCopyKey(sourceSubKey, destSubKey);
            }
        }
    }

    
}
