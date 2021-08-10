
namespace ReturnHome.Server.Network.GameMessages.Messages
{
    class Camera1 : GameMessage
    {
        public Camera1() : base(MessageType.ReliableMessage, GameMessageOpcode.Camera1, GameMessageGroup.SecureWeenieQueue)
        {
            Writer.Write(0x03);
        }
    }
}
