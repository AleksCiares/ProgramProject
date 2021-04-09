using DLPEngineLibrary.Controllers;
using DLPEngineLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebFormServer.Engine
{
    internal static partial class ServerEngine
    {
        internal static List<Client> Clients 
        {
            get { return controlledClients; }
            private set {  } 
        }

        internal static void Start(string path)
        {
            InitServer(path);
            StartListener();
        }

        internal static void Stop()
        {
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
                        packet.HostName = "server";
                        packet.Task = task;
                        packet.Data = JsonController.SerializeToBson(@object);

                        Connection.SendData(stream, JsonController.SerializeToBson(packet));
                        packet = JsonController.DeserializeFromBson<Packet>(Connection.RecieveData(stream));
                        
                        switch(packet.Task)
                        {
                            case "TaskSuccesss":
                                return;
                            case "TaskFailure":
                                continue;
                        }
                    }
                }
                catch (SocketException)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(70));
                }
            } while (true);
        }
    }
}

