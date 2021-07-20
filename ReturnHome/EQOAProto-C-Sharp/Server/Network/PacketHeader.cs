using System;

using ReturnHome.Utilities;

namespace ReturnHome.Server.Network
{
    public class PacketHeader
    {
        //This is the minimum header size we could encounter, but varies through out the game, up to ~14 bytes
        public byte HeaderSize { get; set; }

        public ushort ClientEndPoint { get; set; }
        public ushort TargetEndPoint { get; set; }
        public uint HeaderData { get; set; }
        public PacketHeaderFlags headerFlags { get; set; }
        public PacketBundleFlags bundleFlags { get; set; }
		public bool AckPacket{ get; set; }
        public bool CRCChecksum { get; set; }
        public ushort BundleSize { get; set; }
        public int SessionID { get; set; }
        public uint InstanceID { get; set; }
		public ushort ClientBundleNumber { get; set; }
		public ushort ClientBundleAck { get; set; }
		public ushort ClientMessageAck { get; set; }

        public int Unpack(ReadOnlyMemory<byte> buffer, int offset)
        {
            (ClientEndPoint, offset) = BinaryPrimitiveWrapper.GetLEUShort(buffer, offset);
            (TargetEndPoint, offset) = BinaryPrimitiveWrapper.GetLEUShort(buffer, offset);
            (HeaderData, offset) = Utility_Funcs.Unpack(buffer, offset);
            BundleSize     = (ushort)(HeaderData & 0x7FF);
            headerFlags    = (PacketHeaderFlags)(HeaderData - BundleSize);

            //Verify packet has instance in header
            if (HasHeaderFlag(PacketHeaderFlags.HasInstance))
                (SessionID, offset) = BinaryPrimitiveWrapper.GetLEInt(buffer, offset);

            //if Client is "remote", means it is not "master" anymore and an additional pack value to read which ties into character instanceID
            if (HasHeaderFlag(PacketHeaderFlags.IsRemote))
                (InstanceID, offset) = Utility_Funcs.Unpack(buffer, offset);
			
			else
				InstanceID = 0;
			
			//Set Header size to Offset, offset at this point should the the header size we need
			HeaderSize = (byte)offset;
			
			//Check if it is a transfer packet
			if (!(TargetEndPoint == 0xFFFF))
				//Not Transfer packet, Validate CRC Checksum for packet
				CRCChecksum = buffer.Span.Slice(buffer.Length - 4, 4).SequenceEqual(CRC.calculateCRC(buffer.Span.Slice(0, buffer.Length - 4)));
				
			else
				return offset;

            byte temp;
			//Read Bundle Type, needs abit of a work around....
			(temp, offset) = BinaryPrimitiveWrapper.GetLEByte(buffer, offset);
			bundleFlags = (PacketBundleFlags)temp;

            (ClientBundleNumber, offset) = BinaryPrimitiveWrapper.GetLEUShort(buffer, offset);
			if (HasBundleFlag(PacketBundleFlags.NewProcessReport) || HasBundleFlag(PacketBundleFlags.ProcessMessageAndReport) || 
			    HasBundleFlag(PacketBundleFlags.ProcessReport) || HasBundleFlag(PacketBundleFlags.ProcessAll))
			{
				(ClientBundleAck, offset)	 = BinaryPrimitiveWrapper.GetLEUShort(buffer, offset);
				(ClientMessageAck, offset) = BinaryPrimitiveWrapper.GetLEUShort(buffer, offset);
			}

            return offset;
        }

        public bool HasHeaderFlag(PacketHeaderFlags HeaderFlags) { return (HeaderFlags & headerFlags) == HeaderFlags; }
		
		public bool HasBundleFlag(PacketBundleFlags BundleFlags) { return (BundleFlags & bundleFlags) == BundleFlags; }

    }
}
