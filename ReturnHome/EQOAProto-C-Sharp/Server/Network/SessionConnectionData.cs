
namespace ReturnHome.Server.Network
{
    public class SessionConnectionData
    {
        public ushort PacketSequence { get; set; }
        public ushort MessageSequence { get; set; }


        public SessionConnectionData()
        {
            PacketSequence = 0;
            MessageSequence = 0;
        }
    }
}
