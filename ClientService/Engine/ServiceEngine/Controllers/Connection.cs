using DLPSystem.ClientService.ServiceEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DLPSystem.ClientService.ServiceEngine.Controllers
{
    public static class Connection
    {
        public static TcpClient ConnectToServer(string hostname, int port)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(hostname, port);
            return tcpClient;
        }

        public static TcpListener StartListener(string hostname = null, int port = 27015)
        {
            TcpListener listener = null;

            if (hostname == null)
            {
                listener = new TcpListener(IPAddress.Any, port);
            }
            else
            {
                IPAddress localAddress = IPAddress.Parse(hostname);
                listener = new TcpListener(localAddress, port);
            }

            listener.Start();
            return listener;
        }

        public static void SendData(NetworkStream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public static byte[] RecieveData(NetworkStream stream)
        {
            TransmittedData data = new TransmittedData();
            int bytes = 0;

            do
            {
                bytes = stream.Read(data.Data, data.Offset, data.Freespace);
                data.Offset += bytes;

            } while (stream.DataAvailable);

            return data.Data;
        }
    }
}
