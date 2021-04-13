using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DLPEngineLibrary.Controllers
{
    internal static class Connection
    {
        internal static TcpClient ConnectToHost(string hostname, int port)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(hostname, port);
            return tcpClient;
        }

        internal static TcpListener StartListener(string hostname = null, int port = 29015)
        {
            TcpListener listener = null;

            if (hostname == null)
                listener = new TcpListener(IPAddress.Any, port);
            else
            {
                IPAddress localAddress = IPAddress.Parse(hostname);
                listener = new TcpListener(localAddress, port);
            }

            listener.Start();
            return listener;
        }

        internal static void SendData(NetworkStream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        internal static byte[] RecieveData(NetworkStream stream)
        {
            Models.TransmittedData data = new Models.TransmittedData();
            int bytes = 0;

            do
            {
                bytes = stream.Read(data.Data, data.Offset, data.Freespace);
                data.Offset += bytes;
                if(!stream.DataAvailable)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                        if (!stream.DataAvailable)
                        break;
                }

            } while (true);

            return data.Data;
        }
    }
}
