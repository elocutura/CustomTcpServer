using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CTCP
{
    public class CTCPConnection
    {
        public TcpClient client;
        private NetworkStream stream;
        private IPEndPoint remoteEndPoint;

        private CTCPServer server = null;
        private CTCPConnection(TcpClient client)
        { 
            this.client = client;
            InitClient();
        }

        public CTCPConnection(TcpClient client, CTCPServer server) : this(client)
        { 
            this.server = server;
        }

        public CTCPConnection(IPEndPoint ipEndpoint) : this(new TcpClient())
        {
            remoteEndPoint = ipEndpoint;
        }

        public void Connect()
        {
            client.Connect(remoteEndPoint);
            Start();
        }

        internal void Start()
        {
            OnConnected();

            stream = client.GetStream();

            Task.Run(() => ProcessStream());
        }

        public bool Send(byte[] payload)
        {
            CTCPPacket packet = new CTCPPacket(payload);

            Task.Run(() =>
            {
                try
                {
                    client.Client.Send(packet.GetBuffer());
                }
                catch (SocketException ex)
                {
                    Disconnect();
                }
                catch (ObjectDisposedException ex)
                {
                    Disconnect();
                }
                catch (Exception ex)
                { 
                    OnError(ex);
                }
            });
            return true;
        }

        private void ProcessStream()
        {
            try
            {
                int size;
                int headerSize = CTCPPacket.GetHeaderSize();
                byte[] headerBuff = new byte[headerSize];
                while (true)
                {
                    if (stream.Read(headerBuff, 0, headerSize) > 0)
                    {
                        size = BitConverter.ToInt32(headerBuff, 0);

                        if (size > 0)
                        {
                            var packetPayload = new byte[size];
                            stream.Read(packetPayload, 0, size);

                            OnPayloadReceived(packetPayload);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
        private void InitClient()
        {
            this.client.ReceiveBufferSize = 4096;
            this.client.SendBufferSize = 4096;
        }

        public void Disconnect()
        {
            OnDisconnected();
            stream.Close();
            client.Close();
            client.Dispose();

            if (server != null)
                server.OnDisconnect(this);
        }

        public virtual void OnPayloadReceived(byte[] payload)
        { }

        public virtual void OnConnected()
        { }

        public virtual void OnDisconnected()
        { }

        public virtual void OnError(Exception ex)
        {
            Console.WriteLine("Exception caught in TCP client connection : " + ex.ToString());
        }

    }
}
