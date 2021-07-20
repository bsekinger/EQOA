using System;
using System.IO;
using System.Threading.Channels;

namespace ReturnHome.Server.Network
{
    public class ServerPacket : Packet
    {
        // Can still add packet header to this total
        public static int MaxPacketSize { get; } = 1024;

        /// <summary>
        /// Initializes Channle stuff
        /// </summary>
        public BinaryWriter DataWriter { get; private set; }

        public int currentSize;


        public ServerPacket(int initialCapacity = 32)
        {
            if (DataWriter == null)
            {
                Data = new MemoryStream(initialCapacity);
                DataWriter = new BinaryWriter(Data);
            }
        }

        public void CreateReadyToSendPacket(byte[] buffer, out int size)
        {
            uint payloadChecksum = 0u;

            int offset = Header.HeaderSize;

            if (Data != null && Data.Length > 0)
            {
                var body = Data.GetBuffer();
                Buffer.BlockCopy(body, 0, buffer, offset, (int)Data.Length);
                offset += (int)Data.Length;

                payloadChecksum += Hash32.Calculate(body, (int)Data.Length);
            }

            foreach (ServerPacketFragment fragment in Fragments)
                payloadChecksum += fragment.PackAndReturnHash32(buffer, ref offset);

            size = offset;

            Header.Size = (ushort)(size - PacketHeader.HeaderSize);

            finalChecksum = headerChecksum + payloadChecksum;
            Header.Checksum = headerChecksum + (payloadChecksum ^ issacXor);
            Header.Pack(buffer);
        }
        
        public override string ToString()
        {
            var c = Header.HasFlag(PacketHeaderFlags.EncryptedChecksum) ? $" CRC: {finalChecksum} XOR: {issacXor}" : "";
            return $">>> {Header}{c}".TrimEnd();
        }
        */
    }
}
