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

        public void Unpack(ReadOnlyMemory<byte> buffer, int offset)
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
				return;
			}
			
			MessageType   = temp;
			Size          = BinaryPrimitiveWrapper.GetLEByte(buffer, ref offset);
			MessageNumber = BinaryPrimitiveWrapper.GetLEUShort(buffer, ref offset);
			
			//Message is split across 2+ packets
			if (MessageType == 0xFA)
				Split = true;
        }
        /*
        public void Pack(byte[] buffer, int offset = 0)
        {
            Pack(buffer, ref offset);
        }
        
        public void Pack(byte[] buffer, ref int offset)
        {
            buffer[offset++] = (byte)Sequence;
            buffer[offset++] = (byte)(Sequence >> 8);
            buffer[offset++] = (byte)(Sequence >> 16);
            buffer[offset++] = (byte)(Sequence >> 24);

            buffer[offset++] = (byte)Id;
            buffer[offset++] = (byte)(Id >> 8);
            buffer[offset++] = (byte)(Id >> 16);
            buffer[offset++] = (byte)(Id >> 24);

            buffer[offset++] = (byte)Count;
            buffer[offset++] = (byte)(Count >> 8);

            buffer[offset++] = (byte)Size;
            buffer[offset++] = (byte)(Size >> 8);

            buffer[offset++] = (byte)Index;
            buffer[offset++] = (byte)(Index >> 8);

            buffer[offset++] = (byte)Queue;
            buffer[offset++] = (byte)(Queue >> 8);
        }

        /// <summary>
        /// Returns the Hash32 of the payload added to buffer
        /// </summary>
        public uint PackAndReturnHash32(byte[] buffer, ref int offset)
        {
            Pack(buffer, ref offset);

            return Hash32.Calculate(buffer, offset - HeaderSize, HeaderSize);
        }
        */
    }
}
