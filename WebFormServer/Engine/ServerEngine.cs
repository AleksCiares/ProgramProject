using DLPEngineLibrary.Controllers;
using DLPEngineLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace WebFormServer.Engine
{
    internal static partial class ServerEngine
    {
        static readonly object locker = new object();

        private static string pathToServerFolder = null;
        private static string pathToModulesFolder = null;
        private static string pathToClientsInfoFolder = null;
        private static string pathToClientsConfigFile = null;
        private static List<Client> controlledClients
        {
            get { return controlledClients; }
            set
            {
                lock(locker)
                {
                    controlledClients = value;
                }
            }
        }

        private static void InitServer(string path)
        {
            pathToServerFolder = path;
            pathToClientsInfoFolder = Path.Combine(pathToServerFolder, "ClientsInfo");
            pathToModulesFolder = Path.Combine(pathToServerFolder, "Modules");
            pathToClientsConfigFile = Path.Combine(pathToClientsInfoFolder, "ClientsConfig.json");

            if (!Directory.Exists(pathToServerFolder))
                Directory.CreateDirectory(pathToServerFolder);

            if (!Directory.Exists(pathToClientsInfoFolder))
                Directory.CreateDirectory(pathToClientsInfoFolder);

            if (!Directory.Exists(pathToModulesFolder))
                Directory.CreateDirectory(pathToModulesFolder);

            if (!File.Exists(pathToClientsConfigFile))
                File.Create(pathToClientsConfigFile).Close();

            controlledClients = FileController.ReadObjectFromFile<List<Client>>(
                pathToClientsConfigFile, JsonController.ReadObjectFromJsonFile<List<Client>>);
            if (controlledClients == null) controlledClients = new List<Client>();
        }

        private static void SaveAllConfigs()
        {
            FileController.WriteObjectToFile<List<Client>>(pathToClientsConfigFile,
                controlledClients, JsonController.WriteObjectToJsonFile<List<Client>>);
        }

        private static string GetPathToClientFolder(Client client)
        {
            string path = Path.Combine(pathToClientsInfoFolder,
                client.HostName + "_" + client.IpHost + "_" + client.DnsName);

            return path;
        }

        private static void CreateAllClientDirectiries(Client client)
        {
            string path = GetPathToClientFolder(client);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static Client GetClientConfig(IPEndPoint endPoint, Packet packet)
        {
            string tempDnsName = Dns.GetHostEntry(endPoint.Address).HostName;
            string tempIpHost = endPoint.Address.ToString();

            if (controlledClients.Count > 0)
                foreach (var client in controlledClients)
                    if (client.GUID == packet.GUID)
                    {
                        if (client.HostName != packet.HostName || client.DnsName != tempDnsName ||
                            client.IpHost != tempIpHost)
                        {
                            Directory.Move(GetPathToClientFolder(client), GetPathToClientFolder(new Client()
                            {
                                HostName = packet.HostName,
                                DnsName = tempDnsName,
                                IpHost = tempIpHost,
                                GUID = null,
                            }));

                            client.HostName = packet.HostName;
                            client.DnsName = tempDnsName;
                            client.IpHost = tempIpHost;
                        }
                        return client;
                    }

            // add a new client
            // if the client config file is empty 
            // or the config file does not contain client
            Client tempClient = new Client()
            {
                GUID = packet.GUID,
                HostName = packet.HostName,
                DnsName = tempDnsName,
                IpHost = tempIpHost,
            };
            controlledClients.Add(tempClient);
            CreateAllClientDirectiries(tempClient);
            return tempClient;
        }

        private static Packet ProcessTask(Client client, Packet packet)
        {
            Packet answer = new Packet();
            answer.HostName = "server";
            answer.GUID = null;

            switch (packet.Task)
            {
                #region case CheckServiceConfig
                case "CheckServiceConfig":
                    var serviceConf1 = FileController.ReadObjectFromFile(Path.Combine(GetPathToClientFolder(client),
                        "ServiceConfig.json"), JsonController.ReadObjectFromJsonFile<ServiceConfig>);
                    var serviceConf2 = JsonController.DeserializeFromBson<ServiceConfig>(packet.Data);
                    if (!FileController.CompareObjects(serviceConf1, serviceConf2))
                    {
                        answer.Task = "objectNotConfirmed";
                        answer.Data = JsonController.SerializeToBson(serviceConf1);
                    }
                    else
                    {
                        answer.Task = "objectConfirmed";
                        answer.Data = null;
                    }
                    break;
                #endregion

                #region case CheckModulesConfig
                case "CheckModulesConfig":
                    var modulesConf1 = FileController.ReadObjectFromFile(Path.Combine(GetPathToClientFolder(client),
                        "ModulesConfig.json"), JsonController.ReadObjectFromJsonFile<List<Module>>);
                    var modulesConf2 = JsonController.DeserializeFromBson<List<Module>>(packet.Data);
                    if (!FileController.CompareObjects(modulesConf1, modulesConf2))
                    {
                        answer.Task = "objectNotConfirmed";
                        answer.Data = JsonController.SerializeToBson(modulesConf1);
                    }
                    else
                    {
                        answer.Task = "objectConfirmed";
                        answer.Data = null;
                    }
                    break;
                #endregion
                default:
                    answer.Task = "DoYouWantJoinUS";
                    answer.Data = null;
                    answer.HostName = "lgbtgroups";
                    break;
            }

            return answer;
        }

        private static void StartListener()
        {
            do
            {
                try
                {
                    var listener = Connection.StartListener(IPAddress.Loopback.ToString());
                    do
                    {
                        if (listener.Pending())
                            using (var tcpClient = listener.AcceptTcpClient())
                            {
                                var stream = tcpClient.GetStream();
                                var packet = JsonController.DeserializeFromBson<Packet>
                                    (Connection.RecieveData(stream));

                                Client client = GetClientConfig(((IPEndPoint)tcpClient.Client.RemoteEndPoint), packet);
                                packet = ProcessTask(client, packet);
                                Connection.SendData(stream, JsonController.SerializeToBson(packet));

                                stream.Close();
                                tcpClient.Close();
                            }
                        else
                            Thread.Sleep(TimeSpan.FromSeconds(20));
                        

                    }
                    while (true);
                }
                catch (Exception e)
                {
                    continue;
                }
            } while (true);
        }
            
    }
}