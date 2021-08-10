
namespace ReturnHome.Server.Network.GameMessages
{
    public enum GameMessageOpcode : ushort
    {
        CheckGameDisc       = 0x0000,
        ServerList          = 0x07B3, ///1971
        Camera1             = 0x07D1, ///2001
        Camera2             = 0x07F5, ///2037
        GameEvent           = 0xF7B0,
        GameAction          = 0xF7B1,
        None                = 0x9999
    }
}
