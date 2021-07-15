using System;

using ReturnHome.Utilities;

namespace ReturnHome.Server.Network
{
    public class PacketMessageHeader
    {
        public int HeaderSize { get; set; }
		
		public byte MessageType { get; set; }
        public ushort MessageNumber { get; set; }
        public ushort Size { get; set; }
		public bool Split { get; set; }

        public void Unpack(ReadOnlyMemory<byte> buffer, ref int offset)
        {
            //Read first byte, if it is FF, read an additional byte (Indicates >255byte message
            byte temp = BinaryPrimitiveWrapper.GetLEByte(buffer, ref offset);
			if (temp == 0xFF)
			{
				MessageType   = BinaryPrimitiveWrapper.GetLEByte(buffer, ref offset);
				Size          = BinaryPrimitiveWrapper.GetLEUShort(buffer, ref offset);
				MessageNumber = BinaryPrimitiveWrapper.GetLEUShort(buffer, ref offset);
				
				//Message is split across 2+ packets
				if (MessageType == 0xFA)
					Split = true;
			}
			
			//Single byte message type, may be unreliable/reliable
			else
			{
				if (temp == 0xFA || temp == 0xFB || temp == 0xF9 || temp == 0xFC)
				{
					//Check if split
					if (MessageType == 0xFA)
						Split = true;
			
					//Cec
					MessageType   = temp;
					Size          = BinaryPrimitiveWrapper.GetLEByte(buffer, ref offset);
					
					//FC type is of an unreliable nature and does not have a message#
					if (!(MessageType == 0xFC))
						MessageNumber = BinaryPrimitiveWrapper.GetLEUShort(buffer, ref offset);
				}
				
				//Eventually check for unreliable messages "Character updates" from client
			}
        }
    }
}
