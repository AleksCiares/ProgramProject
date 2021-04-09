using DeviceId;
using System.IO;


namespace DLPSystem.ClientService.ClientEngine
{
    internal partial class ServiceEngine
    {
        internal ServiceEngine(string path, string serverIp, int serverPort)
        {
            pathToServiceFolder = path;
            pathToServiceConfigFile = Path.Combine(path, "ServiceConfig.json");
            pathToModulesConfigFile = Path.Combine(path, "ModulesConfig.json");
            pathToModulesFolder = Path.Combine(path, "Modules");
            serverName = serverIp;
            port = serverPort;
            deviceId = new DeviceIdBuilder().AddMacAddress().ToString();
        }

        internal void Start()
        {
            InitService();
            SynсhronizeDataWithServer();

            StartListener();

            foreach (var module in this._modulesconfig)
            {
                var proc = ExecuteModule(module);
                if (proc != null)
                    this.processes.Add(proc);
            }

        }

        internal void Stop()
        {

        }
    }
}
