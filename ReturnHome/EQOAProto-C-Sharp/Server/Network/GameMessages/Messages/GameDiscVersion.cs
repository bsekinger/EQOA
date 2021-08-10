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
            int offset = 2;
            int GameVersion;
            (Version, offset) = BinaryPrimitiveWrapper.GetLEInt(message.Data, offset);
            if (Version != 0x25 || Version != 0x12)
                pass = false;
            pass =  true;
        }

        public GameDiscVersion(int version) : base(MessageType.ReliableMessage, GameMessageOpcode.CheckGameDisc, GameMessageGroup.SecureWeenieQueue)
        {
            Writer.Write(version);
        }
    }
}
