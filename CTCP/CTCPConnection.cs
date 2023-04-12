using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CTCP
{
    public class CTCPConnection
    {
        public TcpClient client;
        private NetworkStream stream;
        private IPEndPoint remoteEndPoint;

        private CTCPServer server = null;
        private bool connected = false;

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
            connected = true;

            OnConnected();

            stream = client.GetStream();

            Task.Run(() => ProcessStream());
        }

        public bool Send(byte[] payload)
        {
            Task.Run(() =>
            {
                try
                {
                    CTCPPacket packet = new CTCPPacket(payload);
                    if (client.Client != null)
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

        int size;
        int headerSize = CTCPPacket.GetHeaderSize();
        byte[] headerBuff;
        byte[] packetPayload;
        int bytesRead;
        int cumulatedBytesRead;
        int newSize;

        private void ProcessStream()
        {
            try
            {
                
                headerBuff = new byte[headerSize];
                while (connected)
                {
                    if (stream.Read(headerBuff, 0, headerSize) > 0)
                    {
                        size = BitConverter.ToInt32(headerBuff, 0);

                        if (size > 0)
                        {
                            try
                            {
                                packetPayload = new byte[size];
                                bytesRead = stream.Read(packetPayload, 0, size);
                                cumulatedBytesRead = bytesRead;
                                newSize = size - bytesRead;
                                while (cumulatedBytesRead != size)
                                {
                                    bytesRead = stream.Read(packetPayload, cumulatedBytesRead, newSize);
                                    cumulatedBytesRead += bytesRead;
                                    newSize -= bytesRead;
                                }

                                OnPayloadReceived(packetPayload);
                            }
                            catch (Exception ex)
                            { 
                                OnError(ex);
                            }
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
            if (!connected)
                return;

            connected = false;

            if (server != null)
            { 
                server.OnDisconnect(this);  
            }
            stream.Close();
            client.Close();
            OnDisconnected();
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
