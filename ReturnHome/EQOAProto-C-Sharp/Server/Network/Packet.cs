using System.Collections.Generic;
using System.IO;

namespace ReturnHome.Server.Network
{
    public abstract class Packet
    {
        public PacketHeader Header { get; } = new PacketHeader();
        public MemoryStream Data { get; internal set; }
        public BinaryReader binaryReader { get; internal set; }
        public List<PacketMessage> Messages { get; } = new List<PacketMessage>();
    }
}
