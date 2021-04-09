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
        private ManualResetEventSlim closeEvent = new ManualResetEventSlim(false);

        private string serverName = null;
        private int port = default(int);

        private string pathToServiceFolder = null;
        private string pathToServiceConfigFile = null;
        private string pathToModulesConfigFile = null;
        private string pathToModulesFolder = null;
        private string deviceId = null;

        private ServiceConfiguration _serviceconfig = null;
        private readonly object serviceLocker = new object();
        private ServiceConfiguration ServiceConfig
        {
            get 
            {
                lock(serviceLocker)
                    if(_serviceconfig == null)
                    {
                        _serviceconfig = FileController.ReadObjectFromFile(pathToServiceConfigFile,
                            JsonController.ReadObjectFromJsonFile<ServiceConfiguration>);
                        if (_serviceconfig == null) _serviceconfig = new ServiceConfiguration();
                    }

                return _serviceconfig;
            }
            set
            {
                lock(serviceLocker)
                    if(value != default(ServiceConfiguration) && !value.IsEmpty)
                    {
                        _serviceconfig = value;
                        FileController.WriteObjectToFile(pathToServiceConfigFile, value,
                            JsonController.WriteObjectToJsonFile);
                    }
            }
        }

        private List<Module> _modulesconfig = null;
        private readonly object modulesLocker = new object();
        private List<Module> ModulesConfig
        {
            get
            {
                lock(modulesLocker)
                    if(_modulesconfig == null)
                    {
                        _modulesconfig = FileController.ReadObjectFromFile(this.pathToModulesConfigFile,
                            JsonController.ReadObjectFromJsonFile<List<Module>>);
                        if (_modulesconfig == null) _modulesconfig = new List<Module>();
                    }

                return _modulesconfig;
            }
            set
            {
                lock(modulesLocker)
                    if(value != default(List<Module>) && value.Count > 0)
                    {
                        _modulesconfig = value;
                        FileController.WriteObjectToFile(pathToModulesConfigFile, value,
                            JsonController.WriteObjectToJsonFile);
                    }
            }
        }

        private List<Process> processes = new List<Process>();
        
        private void InitService()
        {
            //if (!Directory.Exists(pathToServiceFolder))
            //    Directory.CreateDirectory(pathToServiceFolder);

            //if (!Directory.Exists(pathToModulesFolder))
            //    Directory.CreateDirectory(pathToModulesFolder);

            //if (!File.Exists(pathToServiceConfigFile))
            //    File.Create(pathToServiceConfigFile).Close();

            //if (!File.Exists(pathToModulesConfigFile))
            //    File.Create(pathToModulesConfigFile).Close();
        }

        private T VerifyObjectOnServer<T, T1>(string task, T objectToCheck, string property, T1 comparator)
        {
            double time = 4;
            short attemptNumber = 0;

            TcpClient host = null;
            do
            {
                try
                {
                    using (host = Connection.ConnectToHost(serverName, port))
                    {
                        var stream = host.GetStream();
                        Connection.SendData(stream,
                            JsonController.SerializeToBson(new Packet
                            {
                                GUID = deviceId,
                                HostName = Dns.GetHostName(),
                                Task = task,
                                Data = JsonController.SerializeToBson(ObjectController.GetDataAvailabity(objectToCheck, property, comparator))
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
                catch (Exception e)
                {
                    host?.Close();

                    if (closeEvent.Wait(TimeSpan.FromMinutes(time)))
                        return default(T);

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
            ModulesConfig = VerifyObjectOnServer("CheckModulesConfig", ModulesConfig, "Count", 0); 
            if(ServiceConfig.IsEmpty)
                ServiceConfig = VerifyObjectOnServer("CheckServiceConfig", default(ServiceConfiguration), "IsEmpty", true);
            else
                ServiceConfig = VerifyObjectOnServer("CheckServiceConfig", ServiceConfig, "IsEmpty", true);
        }

        private Packet ProcessTask(Packet packet)
        {
            switch (packet.Task)
            {
                case "installModule":
                    Module module = JsonController.DeserializeFromBson<Module>
                        (packet.Data);
                    FileController.WriteBinaryFile(Path.Combine(pathToModulesFolder,
                        Path.GetFileNameWithoutExtension(module.Name), module.Name), module.Data);
                    ModulesConfig.Add(new Module
                    {
                        Name = module.Name,
                        Version = module.Version,
                        Data = null
                    });
                    ModulesConfig = ModulesConfig;

                    packet.Task = "success";
                    packet.Data = null;
                    break;

                default:
                    break;
            }

            packet.GUID = deviceId;
            packet.HostName = Dns.GetHostName();
            return packet;
        }

        private void StartListener()
        {
            TcpListener listener = null;
            do
            {
                try
                {
                    listener = Connection.StartListener();
                    do
                    {
                        if (listener.Pending())
                            using (var client = listener.AcceptTcpClient())
                            {
                                var stream = client.GetStream();
                                Packet packet = JsonController.DeserializeFromBson<Packet>
                                    (Connection.RecieveData(stream));

                                packet = ProcessTask(packet);
                                Connection.SendData(stream, JsonController.SerializeToBson(packet));

                                stream.Close();
                                client.Close();
                            }
                        else
                            if (closeEvent.Wait(TimeSpan.FromSeconds(20)))
                        {
                            listener.Stop();
                            return;
                        }
                    } while (true);
                }
                catch(Exception e)
                {
                    listener?.Stop();
                    continue;
                }
            } while (true);
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
    }
}
