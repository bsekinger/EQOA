
namespace ReturnHome.Server.Network.GameMessages
{
    public enum GameMessageOpcode : ushort
    {
        CheckGameDisc       = 0x0000,
        GameEvent           = 0xF7B0,
        GameAction          = 0xF7B1,
        None                = 0x9999
    }
}
