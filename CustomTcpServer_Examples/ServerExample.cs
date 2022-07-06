using System;
using System.Net.Sockets;
using CTCP;

namespace CustomTcpServer
{
    internal class ServerExample
    {
        static void Main(string[] args)
        {
            var server = new ExampleServer(5007);

            Console.WriteLine("Server starting...");
            server.Start();
            Console.WriteLine("Server up");

            Console.ReadLine();
            server.Stop();
            Console.WriteLine("Server stopped");
        }

        internal class ExampleServer : CTCPServer
        {
            public ExampleServer(int port = 5100) : base(port) { }

            public override CTCPConnection CreateNewConnection(TcpClient client)
            {
                return new ExampleClient(client, this);
            }
        }

        internal class ExampleClient : CTCPConnection
        {
            public ExampleClient(TcpClient client, CTCPServer server) : base(client, server) { }

            public override void OnPayloadReceived(byte[] payload)
            {
                var buffer = new ByteBuffer(payload);

                Console.WriteLine("-- Payload received --");
                Console.WriteLine(buffer.ReadString());
            }

            public override void OnConnected()
            {
                Console.WriteLine("-- Client connected --");
            }

            public override void OnDisconnected()
            {
                Console.WriteLine("-- Client disconnected --");
            }

            public override void OnError(Exception ex)
            {
                Console.WriteLine("-- Error caught in ExampleClient --");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
