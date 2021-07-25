using ReturnHome.Utilities;
using ReturnHome.Server.Network;

namespace ReturnHome.Server.Network.GameMessages.Messages
{
    public class GameDiscVersion : GameMessage
    {
        public bool pass;
        public int Version;
        public GameDiscVersion(PacketMessage message) : base(0, GameMessageOpcode.CheckGameDisc, GameMessageGroup.SecureWeenieQueue)
        {
            int offset = 0;
            int GameVersion;
            (GameVersion, offset) = BinaryPrimitiveWrapper.GetLEInt(message.Data, offset);
            if (GameVersion != 0x25)
                pass = false;
            pass =  true;
        }

        public GameDiscVersion(int Version) : base(MessageType.ReliableMessage, GameMessageOpcode.CheckGameDisc, GameMessageGroup.SecureWeenieQueue)
        {
            Writer.Write(Version);
        }
    }
}
