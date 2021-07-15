﻿using System.Collections.Generic;

namespace ReturnHome.Server.Network
{
    public abstract class Packet
    {
        public PacketHeader Header { get; } = new PacketHeader();
		public List<PacketMessage> Messages { get; } = new List<PacketMessage>();
    }
}