
namespace ReturnHome.Server.Network.GameMessages.Messages
{
    public class Camera2 : GameMessage
    {
        public Camera2() : base(MessageType.ReliableMessage, GameMessageOpcode.Camera2, GameMessageGroup.SecureWeenieQueue)
        {
            Writer.Write(0x1B);
        }
    }
}
