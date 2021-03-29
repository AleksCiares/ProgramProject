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

namespace ClientService
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

        private void OnWork()
        {
            CheckModulesConfigFile();
            var modules = JsonContoller.ReadJsonFromFile<List<Module>>(this.pathToModulesConfigFile);
            List<Process> processes = ModulesController.ExecuteModules(modules);

            //take out in a separate method
            TcpListener listener = Connection.StartListener();
            
            using(TcpClient client = Connection.ConnectToServer(this.serverName, serverPort))
            {
                NetworkStream stream = client.GetStream();
                Packet packet = new Packet()
                {
                    Task = "checkmodulesconfigfile",
                    Data = null
                };

                Connection.SendData(stream, JsonContoller.SerializeToBson<Packet>(packet));
                packet = JsonContoller.DeserializeFromBson<Packet>(Connection.RecieveData(stream));
            }
        }

       
    }
}
