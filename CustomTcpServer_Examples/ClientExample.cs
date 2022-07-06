using System;
using System.Net;
using CTCP;

namespace CustomTcpServer
{
    internal class ClientExample
    {
        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint serverIpEndpoint = new IPEndPoint(ip, 5007);

            Console.WriteLine("Client connecting...");
            ExampleClient client = new ExampleClient(serverIpEndpoint);
            client.Connect();
            Console.WriteLine("Client connected!");

            ByteBuffer payloadToSend = new ByteBuffer(new byte[64]);
            payloadToSend.WriteString("Hello world!");

            Console.WriteLine("Input anything to send Hello world...");
            Console.WriteLine("Input '!' to stop the client");
            while (Console.ReadLine() != "!")
            {
                Console.WriteLine("Sending Hello world!");
                client.Send(payloadToSend.Buffer);
            }
            client.Disconnect();
        }

        internal class ExampleClient : CTCPConnection
        {
            public ExampleClient(IPEndPoint ipEndpoint) : base(ipEndpoint) { }

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
