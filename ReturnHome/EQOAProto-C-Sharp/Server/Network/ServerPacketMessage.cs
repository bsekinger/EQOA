﻿using System;

namespace ReturnHome.Server.Network
{
    public class ServerPacketMessage : PacketMessage
    {
        public ServerPacketMessage(byte[] data)
        {
            Data = data;
        }

        /*
        /// <summary>
        /// Returns the Hash32 of the payload added to buffer
        /// </summary>
        public uint PackAndReturnHash32(byte[] buffer, ref int offset)
        {
            Header.Size = (ushort)(PacketFragmentHeader.HeaderSize + Data.Length);

            var headerHash32 = Header.PackAndReturnHash32(buffer, ref offset);

            Buffer.BlockCopy(Data, 0, buffer, offset, Data.Length);
            offset += Data.Length;

            return headerHash32 + Hash32.Calculate(Data, Data.Length);
        }
        */
    }
}
