using System;

namespace ReturnHome.Server.Network
{
	//Contains bundle header flags along with BundleTypeFlags
    [Flags]
    public enum PacketHeaderFlags : uint  
    {
        NewProcessMessages      = 0x00000000,
		NewProcessReport        = 0x00000003,
		ProcessMessageAndReport = 0x0000000D,
		ProcessMessages         = 0x00000020,
		ProcessReport           = 0x00000023,
		ProcessAll              = 0x00000063,
        NewInstance             = 0x00002000,
        HasInstance             = 0x00080000,
        IsRemote                = 0x00000800,
        ResetConnection         = 0x00010000
    }
}
