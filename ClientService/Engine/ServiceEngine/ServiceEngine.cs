using ClientService.Controllers;
using ClientService.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceEngine
{
    public partial class ServiceEngine
    {
        private string pathToModulesFolder = null;
        private string pathToModulesConfigFile = null;
        private string serverName = null;
        private int port = 0;
        private List<Module> modules = null;
        private List<Process> processes = null;

        private object CheckAndReadModulesConfigFile()
        {
            if (!File.Exists(this.pathToModulesConfigFile))
            {
                var file = File.Create(pathToModulesConfigFile);
                file.Close();

                return null;
            }

            return (this.modules = JsonController.ReadObjectFromJsonFile<List<Module>>
                (this.pathToModulesConfigFile));
        }

        private void StartListener()
        {
            var listener = Connection.StartListener();

            do
            {
                if (listener.Pending())
                {
                    var client = listener.AcceptTcpClient();
                    var stream = client.GetStream();
                    Packet packet = JsonController.DeserializeFromBson<Packet>
                        (Connection.RecieveData(stream));

                    switch (packet.Task)
                    {
                        case "installmodule":
                            Module module = JsonController.DeserializeFromBson<Module>
                                (packet.Data);
                            ModulesController.InstallModule(this.pathToModulesFolder, module);
                            this.modules.Add(new Module
                            {
                                Name = module.Name,
                                Version = module.Version,
                                AutoStart = module.AutoStart,
                                Data = null
                            });

                            JsonController.WriteObjectToJsonFile<List<Module>>
                                (this.pathToModulesConfigFile, modules);

                            Connection.SendData(stream, JsonController.SerializeToBson<Packet>(
                                new Packet()
                                {
                                    Task = "success",
                                    Data = null
                                }));

                            break;

                        default:
                            break;
                    }

                    stream.Close();
                    client.Close();
                }

            } while (true);


        }
    }
}
