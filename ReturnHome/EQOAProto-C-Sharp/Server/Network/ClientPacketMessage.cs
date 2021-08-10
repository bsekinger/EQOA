using System;
using System.IO;

namespace ReturnHome.Server.Network
{
    public class ClientPacketMessage : PacketMessage
    {
        public bool Unpack(BinaryReader buffer)
        {
            Header.Unpack(buffer);
			
            Data = new Memory<byte>(buffer.ReadBytes(Header.Size));
            return true;
        }
    }
}
