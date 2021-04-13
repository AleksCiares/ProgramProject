using DLPEngineLibrary.Controllers;
using DLPEngineLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebFormServer.Engine
{
    internal static partial class ServerEngine
    {
        internal static List<Client> Clients 
        {
            get { return controlledclients; }
            private set {  } 
        }

        internal static void Start(string path)
        {
            do
            {
                var taskArray = new List<Task>();
                InitServer(path);
                taskArray.Add(Task.Factory.StartNew(StartListener));

                //var bytes = File.ReadAllBytes(@"F:\Postman.exe");
                //Module module = new Module();
                //module.AutoStart = true;
                //module.Data = bytes;
                //module.Name = "Postman.exe";
                //module.Version = "1.1.1.0";
                //module.Path = null;
                //SendTask<Module>(ControlledClients[0].IpHost, 29015, "installModule", module);

                Task.WaitAll(taskArray.ToArray());
                if (closeEvent.IsSet == true)
                    return;
            } while (true);
        }

        internal static void Stop()
        {
            closeEvent.Set();
            SaveAllConfigs();
        }

        internal static void SendTask<T>(string hostname, int port, string task, T @object)
        {
            do
            {
                try
                {
                    using (var host = Connection.ConnectToHost(hostname, port))
                    {
                        var stream = host.GetStream();
                        Packet packet = new Packet();
                        packet.GUID = "server";
                        packet.HostName = "server";
                        packet.Task = task;
                        packet.Data = JsonController.SerializeToBson(@object);

                        Connection.SendData(stream, JsonController.SerializeToBson(packet));
                        packet = JsonController.DeserializeFromBson<Packet>(Connection.RecieveData(stream));
                        
                        switch(packet.Task)
                        {
                            case "tasksuccess":
                                return;
                            case "taskfailure":
                                continue;
                        }
                    }
                }
                catch (Exception e)
                {
                    Errors(null, e);
                    if (closeEvent.Wait(TimeSpan.FromSeconds(20)))
                        return;
                    continue;
                }
            } while (true);
        }
    }
}

