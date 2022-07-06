using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace CTCP
{
    public class CTCPServer
    {
        private int port;
        private TcpListener listener;
        public List<CTCPConnection> connections = new List<CTCPConnection>();

        public CTCPServer(int port = 5100)
        { 
            this.port = port;
        }

        public void Start()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, this.port);

                listener.Start();
                Console.WriteLine("Server started");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    var customConnection = CreateNewConnection(client);
                    connections.Add(customConnection);

                    Console.WriteLine("New connection");
                    Console.WriteLine("Total connections active " + connections.Count);

                    // New connection
                    Task.Run(() =>
                    {
                        customConnection.Start();
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server caught an exception : " + ex.Message);
            }  
        }

        public void Stop()
        {
            foreach (var item in connections)
            {
                item.Disconnect();
            }

            listener.Stop();
        }

        public virtual CTCPConnection CreateNewConnection(TcpClient client)
        {
            return new CTCPConnection(client, this);
        }

        internal void OnDisconnect(CTCPConnection connection)
        { 
            connections.Remove(connection);
            OnClientDisconnect(connection);
        }

        public virtual void OnClientDisconnect(CTCPConnection connection)
        { 
        
        }
    }
}
