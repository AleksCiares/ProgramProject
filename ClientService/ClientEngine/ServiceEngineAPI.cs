using DeviceId;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DLPSystem.ClientService.ClientEngine
{
    internal partial class ServiceEngine
    {
        internal ServiceEngine(string path, string serverIp, short serverPort)
        {
            pathToServiceFolder = path;
            pathToServiceConfigFile = Path.Combine(path, "ServiceConfig.json");
            pathToModulesConfigFile = Path.Combine(path, "ModulesConfig.json");
            pathToModulesFolder = Path.Combine(path, "Modules");
            pathToLogFile = Path.Combine(path, "Errorslog.txt");
            serverName = serverIp;
            port = serverPort;
            deviceId = new DeviceIdBuilder().AddMacAddress().ToString();
            MainEvent += ServiceEngine_mainEvent;
            Errors += ServiceEngine_Errors;
        }

        internal void Start()
        {
            do
            {
                var taskArray = new List<Task>();
                taskArray.Add(Task.Factory.StartNew(SynсhronizeDataWithServer));
                taskArray.Add(Task.Factory.StartNew(StartListener));  
                ExecuteModules();

                //SynсhronizeDataWithServer();
                //ExecuteModules();
                //StartListener();

                Task.WaitAll(taskArray.ToArray());
                if (CloseEvent.IsSet == true)
                {
                    foreach (var process in Processes)
                        KillProcess(process);
                    return;
                }
            } while (true);

        }

        internal void Stop()
        {
            CloseEvent.Set();
        }
    }
}
