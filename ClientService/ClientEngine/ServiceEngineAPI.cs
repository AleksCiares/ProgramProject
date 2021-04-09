using DeviceId;
using DLPEngineLibrary.Controllers;
using DLPEngineLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DLPSystem.ClientService.ClientEngine
{
    internal partial class ServiceEngine
    {
        internal ServiceEngine(string pathToServiceFolder, string serverName, int port)
        {
            this.pathToServiceFolder = pathToServiceFolder;
            this.pathToServiceConfigFile = Path.Combine(pathToServiceFolder, "ServiceConfig.json");
            this.pathToModulesConfigFile = Path.Combine(pathToServiceFolder, "ModulesConfig.json");
            this.pathToModulesFolder = Path.Combine(pathToServiceFolder, "Modules");
            this.serverName = serverName;
            this.port = port;
            this.deviceId = new DeviceIdBuilder().AddMacAddress().ToString();
        }

        internal void Start()
        {
            InitService();
            SynсhronizeDataWithServer();

            this.StartListener();

            foreach (var module in this.modulesConfig)
            {
                var proc = ExecuteModule(module);
                if (proc != null)
                    this.processes.Add(proc);
            }

        }
    }
}
