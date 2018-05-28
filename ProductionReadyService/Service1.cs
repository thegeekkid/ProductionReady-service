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
            Process proc = new Process();
            proc.StartInfo.FileName = @"C:\Windows\System32\net.exe";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.Arguments = @"start wuauserv_prod";
            proc.Start();
            proc.WaitForExit();
        }

        protected void runner()
        {
            Process proc = new Process();
            proc.StartInfo.FileName = @"C:\Windows\System32\net.exe";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            do
            {
                if (pmode)
                {
                    if (Days.Contains(((int)DateTime.Now.DayOfWeek).ToString()))
                    {
                        proc.StartInfo.Arguments = @"stop wuauserv_prod";
                        proc.Start();
                        proc.WaitForExit();
                    }else
                    {
                        proc.StartInfo.Arguments = @"start wuauserv_prod";
                        proc.Start();
                        proc.WaitForExit();
                    }
                }else
                {
                    proc.StartInfo.Arguments = @"start wuauserv_prod";
                    proc.Start();
                    proc.WaitForExit();
                }
                // Sleep for 15 minutes
                Thread.Sleep(900000);
            } while (1 == 1);
        }

        public bool pmode = false;
        public string Days = "";

        protected void readSettings()
        {
            RegistryKey root = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Semrau Software Consulting").OpenSubKey("ProductionReady");

            pmode = bool.Parse(root.GetValue("pmode").ToString());

            Days = root.GetValue("ProductionDays").ToString();
            
            if (Days.Split(',').Count() > 3)
            {
                pmode = false;
            }
        }
    }
}
