using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DLPSystem.ClientService
{
    public partial class DLPClientService : ServiceBase
    {
        public DLPClientService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            
        }

        protected override void OnStop()
        {

        }



       
    }
}
