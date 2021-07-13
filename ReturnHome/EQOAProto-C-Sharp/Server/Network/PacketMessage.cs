using System;

namespace ReturnHome.Server.Network
{
    public abstract class PacketMessage
    {
        public PacketMessageHeader Header { get; } = new PacketMessageHeader();
        public ReadOnlyMemory<byte> Data { get; protected set; }
        public int Length => Header.HeaderSize + Data.Length;
    }
}
