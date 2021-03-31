using DLPSystem.WebFormServer.Engine.Models;
using System.Collections.Generic;
using System.IO;
using System.Net;
using WebFormServer.Engine.Controllers;
using WebFormServer.Engine.Models;

namespace WebFormServer.Engine
{
    public partial class ServerEngine
    {
        private string pathToClientsConfigFile = @"F:\ClientsConfig.json";
        private List<Client> clients = null;

        private void CheckConfigFiles()
        {
            if (!File.Exists(this.pathToClientsConfigFile))
            {
                var file = File.Open(this.pathToClientsConfigFile,
                    FileMode.OpenOrCreate);
                file.Close();
                this.clients = new List<Client>();

                return;
            }

            this.clients = JsonController.ReadObjectFromJsonFile<List<Client>>
                        (pathToClientsConfigFile);
            if (this.clients == null)
                clients = new List<Client>();
        }

        private void StartListener()
        {
            //Create tcp listening socket on ip/port pair
            //and listen incoming connections
            var listener = Connection.StartListener(IPAddress.Loopback.ToString());

            do
            {
                var client = listener.AcceptTcpClient();

                //checking for a registered client
                if (this.clients.Count > 0)
                {
                    bool isExist = false;
                    foreach (var cl in this.clients)
                    {
                        if (((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString() ==
                            cl.IpHost)
                        {
                            isExist = true;
                            break;
                        }
                    }
                    if (isExist == false)
                        this.clients.Add(new Client()
                        {
                            Name = ((IPEndPoint)client.Client.RemoteEndPoint)
                            .Address.ToString(),
                            IpHost = ((IPEndPoint)client.Client.RemoteEndPoint)
                            .Address.ToString(),
                            Modules = null
                        });
                }
                // or just add a new client
                // if the client config file is empty 
                else
                {
                    this.clients.Add(new Client()
                    {
                        Name = ((IPEndPoint)client.Client.RemoteEndPoint)
                           .Address.ToString(),
                        IpHost = ((IPEndPoint)client.Client.RemoteEndPoint)
                           .Address.ToString(),
                        Modules = null
                    });
                }

                JsonController.WriteObjectToJsonFile<List<Client>>
                    (this.pathToClientsConfigFile, this.clients);

                var stream = client.GetStream();
                var packet = JsonController.DeserializeFromBson<Packet>
                    (Connection.RecieveData(stream));

            }
            while (true);
        }
    }
}