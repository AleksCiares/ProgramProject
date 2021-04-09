using DeviceId;
using DLPEngineLibrary.Controllers;
using DLPEngineLibrary.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DLPSystem.ClientService.ClientEngine
{
    internal partial class ServiceEngine
    {
        private string pathToServiceFolder = null;
        private string pathToServiceConfigFile = null;
        private string pathToModulesConfigFile = null;
        private string pathToModulesFolder = null;
        private string deviceId = null;

        private string serverName = null;
        private int port = 0;

        private ServiceConfig serviceConfig = null;
        private List<Module> modulesConfig = null;
        private List<Process> processes = null;
        
        private void InitService()
        {
            if (!Directory.Exists(this.pathToServiceFolder))
                Directory.CreateDirectory(this.pathToServiceFolder);

            if (!File.Exists(this.pathToServiceFolder))
                File.Create(this.pathToServiceConfigFile).Close();

            if (!File.Exists(this.pathToModulesConfigFile))
                File.Create(this.pathToModulesConfigFile).Close();

            if (!Directory.Exists(this.pathToModulesFolder))
                Directory.CreateDirectory(this.pathToModulesFolder);

            serviceConfig = FileController.ReadObjectFromFile(pathToServiceConfigFile,
                JsonController.ReadObjectFromJsonFile<ServiceConfig>);
            if (serviceConfig == null) serviceConfig = new ServiceConfig();

            modulesConfig = FileController.ReadObjectFromFile(this.pathToModulesConfigFile,
                JsonController.ReadObjectFromJsonFile<List<Module>>);
            if (modulesConfig == null) modulesConfig = new List<Module>();
        }

        private T VerifyObjectOnServer<T>(string task, T objectToCheck)
        {
            double time = 4;
            short attemptNumber = 0;

            do
            {
                try
                {
                    using (var host = Connection.ConnectToHost(serverName, port))
                    {
                        var stream = host.GetStream();
                        Connection.SendData(stream,
                            JsonController.SerializeToBson(new Packet
                            {
                                GUID = this.deviceId,
                                HostName = Dns.GetHostName(),
                                Task = task,
                                Data = JsonController.SerializeToBson(objectToCheck)
                            }));

                        var packet = JsonController.DeserializeFromBson<Packet>
                            (Connection.RecieveData(stream));

                        switch (packet.Task)
                        {
                            case "objectConfirmed":
                                stream.Close();
                                host.Close();
                                return default(T);

                            case "objectNotConfirmed":
                                stream.Close();
                                host.Close();
                                return JsonController.DeserializeFromBson<T>
                                    (packet.Data);
                        }
                    }
                }
                catch (SocketException e)
                {
                    Thread.Sleep(TimeSpan.FromMinutes(time));
                    if (attemptNumber++ == 20)
                    {
                        time = 65;
                        attemptNumber = 0;
                    }
                    else
                        time = 4;
                }
            } while (true);
        }

        private void SynсhronizeDataWithServer()
        {
            List<Module> temp;
            if (modulesConfig.Count <= 0)
                temp = VerifyObjectOnServer("CheckModulesConfig", default(List<Module>));
            else
                temp = VerifyObjectOnServer("CheckModulesConfig", modulesConfig);
            if (temp != null)
                modulesConfig = temp;

            ServiceConfig temp1;
            if (serviceConfig.IsEmpty())
                temp1 = VerifyObjectOnServer("CheckServiceConfig", default(ServiceConfig));
            else
                temp1 = VerifyObjectOnServer("CheckServiceConfig", serviceConfig);
            if (temp1 != null)
                serviceConfig = temp1;
        }

        private Process ExecuteModule(Module module)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = (Path.Combine(this.pathToModulesFolder,
                module.Name));
            try
            {
                return Process.Start(startInfo);
            }
            catch(Exception)
            {
                return null;
            }
        }

        private void WriteBinaryFile(Module module)
        {
            using(BinaryWriter writer = new BinaryWriter(File.Open(
                Path.Combine(this.pathToModulesFolder, module.Name), FileMode.Create)))
            {
                writer.Write(module.Data);
                writer.Close();
            }
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
                        case "installModule":
                            Module module = JsonController.DeserializeFromBson<Module>
                                (packet.Data);
                            WriteBinaryFile(module);
                            this.modulesConfig.Add(new Module
                            {
                                Name = module.Name,
                                Version = module.Version,
                                Data = null
                            });

                            Connection.SendData(stream, JsonController.SerializeToBson<Packet>(
                                new Packet()
                                {
                                    GUID = this.deviceId,
                                    HostName = Dns.GetHostName(),
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
