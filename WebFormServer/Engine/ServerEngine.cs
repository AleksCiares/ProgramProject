using DLPEngineLibrary.Controllers;
using DLPEngineLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WebFormServer.Engine
{
    internal static partial class ServerEngine
    {
        private static readonly object locker = new object();
        private static ManualResetEventSlim closeEvent = new ManualResetEventSlim(false);

        private static event EventHandler<Exception> Errors;
        private static readonly object errorsLocker = new object();
        private static void ServiceEngine_Errors(object sender, Exception e)
        {
            lock (errorsLocker)
                FileController.WriteLogInfo(pathToLogFile, e.ToString());
        }

        private static string pathToServerFolder = null;
        private static string pathToModulesFolder = null;
        private static string pathToClientsInfoFolder = null;
        private static string pathToClientsConfigFile = null;
        private static string pathToLogFile = null;

        private static List<Client> controlledclients = null;
        private static List<Client> ControlledClients
        {
            get
            {
                lock (locker)
                    if (controlledclients == null)
                    {
                        controlledclients = FileController.ReadObjectFromFile(pathToClientsConfigFile,
                            JsonController.ReadObjectFromJsonFile<List<Client>>);
                        if (controlledclients == null) controlledclients = new List<Client>();
                    }

                return controlledclients;
            }
            set
            {
                lock (locker)
                    if (value != default(List<Client>) && value.Count > 0)
                    {
                        controlledclients = value;
                        FileController.WriteObjectToFile(pathToClientsConfigFile,
                            value, JsonController.WriteObjectToJsonFile);
                    }
            }
        }

        private static void InitServer(string path)
        {
            pathToServerFolder = path;
            pathToClientsInfoFolder = Path.Combine(pathToServerFolder, "ClientsInfo");
            pathToModulesFolder = Path.Combine(pathToServerFolder, "Modules");
            pathToClientsConfigFile = Path.Combine(pathToClientsInfoFolder, "ClientsConfig.json");
            pathToLogFile = Path.Combine(pathToServerFolder, "ErrorsLog.txt");
        }

        private static void SaveAllConfigs()
        {
            FileController.WriteObjectToFile<List<Client>>(pathToClientsConfigFile,
                ControlledClients, JsonController.WriteObjectToJsonFile<List<Client>>);
        }

        private static string GetPathToClientFolder(Client client)
        {
            string path = Path.Combine(pathToClientsInfoFolder,
                client.HostName + "_" + client.IpHost + "_" + client.DnsName);

            return path;
        }

        private static Client GetClientConfig(IPEndPoint endPoint, Packet packet)
        {
            string tempDnsName = null;
            try
            {
                tempDnsName = Dns.GetHostEntry(endPoint.Address).HostName;
            }
            catch(Exception e)
            {
                Errors(null, e);
                tempDnsName = packet.HostName;
            }
            
            string tempIpHost = endPoint.Address.ToString();

            if (ControlledClients.Count > 0)
                foreach (var client in ControlledClients)
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

                            ControlledClients = ControlledClients;
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
            ControlledClients.Add(tempClient);
            ControlledClients = ControlledClients;
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
                        "ServiceConfig.json"), JsonController.ReadObjectFromJsonFile<ServiceConfiguration>);
                    var serviceConf2 = JsonController.DeserializeFromBson<ServiceConfiguration>(packet.Data);
                    if (!ObjectController.CompareObjects(serviceConf1, serviceConf2))
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
                    if (!ObjectController.CompareObjects(modulesConf1, modulesConf2))
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
            TcpListener listener = null;
            do
            {
                try
                {
                    listener = Connection.StartListener();
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
                            if (closeEvent.Wait(TimeSpan.FromSeconds(5)))
                        {
                            listener?.Stop();
                            return;
                        }
                    } while (true);
                }
                catch (Exception e)
                {
                    listener?.Stop();
                    Errors(null, e);
                    continue;
                }
            } while (true);
        }
    }
}