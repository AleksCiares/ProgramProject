using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ClientService
{
    public partial class DLPClientService : ServiceBase
    {
        private StreamWriter file;

        public DLPClientService()
        {
            InitializeComponent();
            //eventLog1 = new System.Diagnostics.EventLog();
            //if(!System.Diagnostics.EventLog.SourceExists("MySource"))
            //{
            //    System.Diagnostics.EventLog.CreateEventSource("MySource", "MyNewLog");
            //}
            //eventLog1.Source = "MySource";
            //eventLog1.Log = "MyNewLog";

            //this.CanStop = false;
            //this.CanPauseAndContinue = false;

            this.file = new StreamWriter(new FileStream("F:\\MyFirstService.log",
                System.IO.FileMode.Append));
            this.file.WriteLine("Service started");
            this.file.Flush();
        }

        protected override void OnStart(string[] args)
        {
            //DateTime localDate = DateTime.Now;
            //String cultureName = "be-BY";
            //eventLog1.WriteEntry($"{cultureName} {localDate.ToString(new CultureInfo(cultureName))} :" +
            //    $"service started");

            this.file.Write("Service stopped");
            this.file.Flush();
            this.file.Close();
        }

        protected override void OnStop()
        {
        }
    }
}
