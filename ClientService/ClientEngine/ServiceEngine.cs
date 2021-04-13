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
        private ManualResetEventSlim CloseEvent = new ManualResetEventSlim(false);

        private event EventHandler<string> MainEvent;
        private void ServiceEngine_mainEvent(object sender, string e)
        {
            switch (e)
            {
                case "moduleschanged":
                    ExecuteModules();
                    break;

                case "servicechanged":

                break;
            }
        }

        private event EventHandler<Exception> Errors;
        private readonly object errorsLocker = new object();
        private void ServiceEngine_Errors(object sender, Exception e)
        {
            lock (errorsLocker)
                FileController.WriteLogInfo(pathToLogFile, e.ToString());
        }

        private readonly string serverName = null;
        private readonly short port = default(short);

        private readonly string pathToServiceFolder = null;
        private readonly string pathToModulesFolder = null;
        private readonly string pathToServiceConfigFile = null;
        private readonly string pathToModulesConfigFile = null;
        private readonly string pathToLogFile = null;
        private readonly string deviceId = null;

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
                        MainEvent(null, "servicechanged");
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
                lock (processesLocker)
                    lock (modulesLocker)
                        if (value != default(List<Module>) && value.Count > 0)
                        {
                            _modulesconfig = value;
                            FileController.WriteObjectToFile(pathToModulesConfigFile, value,
                                JsonController.WriteObjectToJsonFile);
                            MainEvent(null, "moduleschanged");
                        }
            }
        }


        private List<Process> processes = new List<Process>();
        private readonly object processesLocker = new object();
        private List<Process> Processes
        {
            get { return processes; }
            set { processes = value; }
        }
        
        private void InitService()
        {
            if (!Directory.Exists(pathToServiceFolder))
                Directory.CreateDirectory(pathToServiceFolder);

            if (!Directory.Exists(pathToModulesFolder))
                Directory.CreateDirectory(pathToModulesFolder);

            if (!File.Exists(pathToServiceConfigFile))
                File.Create(pathToServiceConfigFile).Close();

            if (!File.Exists(pathToModulesConfigFile))
                File.Create(pathToModulesConfigFile).Close();
        }

        /// <summary>
        /// Verify the <paramref name="objectToCheck"/> of the configuration class type of <typeparamref name="T"/> on the server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="task"></param>
        /// <param name="objectToCheck"></param>
        /// <param name="property"></param>
        /// <param name="comparator"></param>
        /// <returns>
        /// Returns object of type <typeparamref name="T"/> if the object is not verified, otherwise
        /// returns null
        /// </returns>
        private T SendTaskToHost<T, T1>(string task, T objectToCheck, string property, T1 comparator)
        {
            double time = 5;
            short attemptNumber = 0;
            TcpClient host = null;

            do
            {
                try
                {
                    using (host = Connection.ConnectToHost(serverName, port))
                    {
                        var stream = host.GetStream();
                        Connection.SendData(stream, JsonController.SerializeToBson(new Packet
                            {
                                GUID = deviceId,
                                HostName = Dns.GetHostName(),
                                Task = task,
                                Data = JsonController.SerializeToBson(ObjectController.GetDataAvailabity(objectToCheck, property, comparator))
                            }));

                        var packet = JsonController.DeserializeFromBson<Packet>
                            (Connection.RecieveData(stream));
                        stream.Close();
                        host.Close();
                        
                        return JsonController.DeserializeFromBson<T>(packet.Data);
                    }
                }
                catch (Exception e)
                {
                    host?.Close();
                    Errors(null, e);

                    if (CloseEvent.Wait(TimeSpan.FromSeconds(time)))
                        return default(T);

                    if (attemptNumber++ == 20)
                    {
                        time = 65;
                        attemptNumber = 0;
                    }
                    else
                        time = 5;
                }
            } while (true);
        }

        private void SynсhronizeDataWithServer()
        {
            ServiceConfig = SendTaskToHost("CheckServiceConfig", ServiceConfig, "IsEmpty", true);
            ModulesConfig = SendTaskToHost("CheckModulesConfig", ModulesConfig, "Count", 0);
        }

        private Packet ProcessTask(Packet packet)
        {
            switch (packet.Task)
            {
                case "installmodule":
                    Module module = JsonController.DeserializeFromBson<Module>(packet.Data);
                    string path = Path.Combine(Path.GetFileNameWithoutExtension(module.Name), module.Name);
                    FileController.WriteBinaryFile(Path.Combine(pathToModulesFolder, path), module.Data);
                    ModulesConfig.Add(new Module
                    {
                        Name = module.Name,
                        Path = path,
                        AutoStart = module.AutoStart,
                        Version = module.Version,
                        Data = null
                    });
                    ModulesConfig = ModulesConfig;

                    packet.Task = "tasksuccess";
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
                            if (CloseEvent.Wait(TimeSpan.FromSeconds(5)))
                        {
                            listener.Stop();
                            return;
                        }
                    } while (true);
                }
                catch(Exception e)
                {
                    listener?.Stop();
                    Errors(null, e);
                    continue;
                }
            } while (true);
        }

        /// <summary>
        /// ! fix the termination of already included processes
        /// </summary>
        private void ExecuteModules()
        {
            Process proc = null;

            lock (processesLocker)
                foreach (var module in ModulesConfig)
                {
                    foreach (var temp in Processes)
                        if (temp.ProcessName == module.Name)
                            goto next_iter;

                    if (module.AutoStart)
                        proc = ExecuteModule(module);
                    if (proc != null)
                        Processes.Add(proc);

                    next_iter:;
                }

            lock(processesLocker)
                foreach(var process in Processes)
                {
                    foreach (var module in ModulesConfig)
                        if (process.ProcessName == module.Name)
                            goto next_iter1;

                    process.Kill();

                    next_iter1:;
                }
        }

        private Process ExecuteModule(Module module)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = (Path.Combine(pathToModulesFolder,
                module.Path));
            try
            {
                return Process.Start(startInfo);
            }
            catch(Exception)
            {
                return null;
            }
        }

        private void KillProcess(Process process)
        {
            Processes.Remove(process);
            process?.Kill();
        }
    }
}
