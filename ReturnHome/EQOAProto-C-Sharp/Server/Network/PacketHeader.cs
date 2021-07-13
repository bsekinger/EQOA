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
        public uint headerFlags { get; set; }
        public PacketHeaderFlags bundleFlags { get; set; }
		public bool AckPacket{ get; set; }
        public bool CRCChecksum { get; set; }
        public ushort BundleSize { get; set; }
        public int SessionID { get; set; }
        public uint InstanceID { get; set; }
		public ushort ClientBundleNumber { get; set; }
		public ushort ClientBundleAck { get; set; }
		public ushort ClientMessageAck { get; set; }

        public void Unpack(ReadOnlyMemory<byte> buffer, ref int offset)
        {
            ClientEndPoint = BinaryPrimitiveWrapper.GetLEUShort(buffer, ref offset);
            TargetEndPoint = BinaryPrimitiveWrapper.GetLEUShort(buffer, ref offset);
            HeaderData     = Utility_Funcs.Unpack(buffer, ref offset);
            BundleSize     = (ushort)(HeaderData & 0x7FF);
            headerFlags    = HeaderData - BundleSize;

            //Verify packet has instance in header
            if (HasHeaderFlag(PacketHeaderFlags.HasInstance))
                SessionID = BinaryPrimitiveWrapper.GetLEInt(buffer, ref offset);

            //if Client is "remote", means it is not "master" anymore and an additional pack value to read which ties into character instanceID
            if (HasHeaderFlag(PacketHeaderFlags.IsRemote))
                InstanceID = Utility_Funcs.Unpack(buffer, ref offset);
			
			else
				InstanceID = 0;
			
			//Set Header size to Offset, offset at this point should the the header size we need
			HeaderSize = (byte)offset;
			
			//Check if it is a transfer packet
			if (!(TargetEndPoint == 0xFFFF))
				//Not Transfer packet, Validate CRC Checksum for packet
				CRCChecksum = buffer.Span.Slice(buffer.Length - 4, 4).SequenceEqual(CRC.calculateCRC(buffer.Span.Slice(0, buffer.Length - 4)));
				
			else
				return;
			
			//Read Bundle Type
			bundleFlags    = (PacketHeaderFlags)BinaryPrimitiveWrapper.GetLEByte(buffer, ref offset);
			
			ClientBundleNumber = BinaryPrimitiveWrapper.GetLEUShort(buffer, ref offset);
			if (HasBundleFlag(PacketHeaderFlags.NewProcessReport) || HasBundleFlag(PacketHeaderFlags.ProcessMessageAndReport) || 
			    HasBundleFlag(PacketHeaderFlags.ProcessReport) || HasBundleFlag(PacketHeaderFlags.ProcessAll))
			{
				ClientBundleAck	 = BinaryPrimitiveWrapper.GetLEUShort(buffer, ref offset);
				ClientMessageAck = BinaryPrimitiveWrapper.GetLEUShort(buffer, ref offset);
			}
        }

        public bool HasHeaderFlag(PacketHeaderFlags headerFlags) { return (headerFlags & headerFlags) != 0; }
		
		public bool HasBundleFlag(PacketHeaderFlags bundleFlags) { return (bundleFlags & bundleFlags) != 0; }

    }
}
