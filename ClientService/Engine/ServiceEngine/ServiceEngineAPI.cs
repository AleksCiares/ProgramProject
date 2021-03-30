using ClientService.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceEngine
{
    public partial class ServiceEngine
    {
        ServiceEngine(string path, string serverName, int port)
        {
            this.pathToModulesFolder = path;
            this.serverName = serverName;
            this.port = port;

            this.pathToModulesConfigFile = Path.Combine(this.pathToModulesFolder,
                "ModulesConfigFile.json");
        }

        public void Start()
        {
            if (this.CheckAndReadModulesConfigFile() != null)
                this.processes = ModulesController.ExecuteModules(this.pathToModulesFolder,
                    this.modules);

            this.StartListener();
        }

        public static void Main()
        {
            ServiceEngine serviceEngine = new ServiceEngine(@"E:\test", "127.0.0.1", 27015);

            serviceEngine.Start();
        }
    }
}
