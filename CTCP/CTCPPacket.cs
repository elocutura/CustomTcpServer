using System;

namespace CTCP
{
    public class CTCPPacket
    {
        private int dataSize;
        private byte[] buffer;

        public CTCPPacket(byte[] data)
        { 
            dataSize = data.Length;
            buffer = new byte[GetHeaderSize() + dataSize];
            var sizeArray = BitConverter.GetBytes(dataSize);
            
            Buffer.BlockCopy(sizeArray, 0, buffer, 0, sizeArray.Length);
            Buffer.BlockCopy(data, 0, buffer, sizeArray.Length, data.Length);
        }

        public byte[] GetBuffer()
        {
            return buffer;        
        }

        public byte[] GetPayload()
        { 
            var newBuff = new byte[dataSize];
            Buffer.BlockCopy(buffer, GetHeaderSize(), newBuff, 0, dataSize);

            return newBuff;
        }

        public static int GetHeaderSize()
        {
            // 4  = uint32 packet size 
            return 4;
        }
    }
}
