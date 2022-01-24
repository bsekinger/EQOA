namespace ReturnHome.Server.Network
{
    class PacketSegmentHeader
    {
        public uint Flags;
        public int Size;
        public uint InstanceID;
        public uint data;

        public void Read()
        {

        }
    }
}
