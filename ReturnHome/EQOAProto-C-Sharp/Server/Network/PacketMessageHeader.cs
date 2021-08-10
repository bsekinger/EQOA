using System;
using System.IO;
using ReturnHome.Utilities;

namespace ReturnHome.Server.Network
{
    public class PacketMessageHeader
    {
        public int HeaderSize { get; set; }
		
		public byte MessageType { get; set; }
        public ushort MessageNumber { get; set; }
        public ushort Size { get; set; }
		public int Count { get; set; }
        public int Index { get; set; }
        public bool Split { get; set; }

        public void Unpack(BinaryReader buffer)
        {
            //Read first byte, if it is FF, read an additional byte (Indicates >255byte message
            byte temp= buffer.ReadByte();
			if (temp == 0xFF)
			{
				MessageType   = buffer.ReadByte();
                Size          = buffer.ReadUInt16();
				MessageNumber = buffer.ReadUInt16();

                //Message is split across 2+ packets
                if (MessageType == 0xFA)
					Split = true;
			}
			
			//Single byte message type, may be unreliable/reliable
			else
			{
				if (temp == 0xFA || temp == 0xFB || temp == 0xF9 || temp == 0xFC)
				{
                    MessageType = temp;

                    //Check if split
                    if (MessageType == 0xFA)
						Split = true;
			
					Size = buffer.ReadByte();

                    //FC type is of an unreliable nature and does not have a message#
                    if (!(MessageType == 0xFC))
						MessageNumber = buffer.ReadUInt16();
                }
				
				//Eventually check for unreliable messages "Character updates" from client
			}
        }
    }
}
