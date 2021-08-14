﻿using System;
using System.IO;
using System.Linq;
using ReturnHome.Utilities;

namespace ReturnHome.Server.Network
{
    public class PacketHeader
    {
        public ushort ClientEndPoint { get; set; }
        public ushort TargetEndPoint { get; set; }
        public uint HeaderData { get; set; }
        public PacketHeaderFlags headerFlags { get; set; }
        public PacketBundleFlags bundleFlags { get; set; }
		public bool NewInstance {get; private set;} = false;
        public bool CRCChecksum { get; private set; } = false;
        public bool RDPReport { get; private set; } = false;
		public bool ProcessMessage { get; private set; } = false;
        public bool CancelSession { get; private set; } = false;
        public ushort BundleSize { get; set; }
        public uint SessionID { get; set; }
        public uint InstanceID { get; set; }
        public ushort ClientBundleNumber { get; set; }
        public ushort ClientBundleAck { get; set; }
        public ushort ClientMessageAck { get; set; } 

        public void Unpack(ReadOnlyMemory<byte> buffer, ref int offset)
        {
            ClientEndPoint = buffer.GetLEUShort(ref offset);
            TargetEndPoint = buffer.GetLEUShort(ref offset);
            HeaderData = buffer.Get7BitEncodedInt(ref offset);
            BundleSize = (ushort)(HeaderData & 0x7FF);
            headerFlags = (PacketHeaderFlags)(HeaderData - BundleSize);

            //Verify packet has instance in header
            if (HasHeaderFlag(PacketHeaderFlags.HasInstance))
                SessionID = buffer.GetLEUInt(ref offset);

            //if Client is "remote", means it is not "master" anymore and an additional pack value to read which ties into character instanceID
            if (HasHeaderFlag(PacketHeaderFlags.IsRemote))
                InstanceID = (uint)buffer.Get7BitDoubleEncodedInt(ref offset);

            else
                InstanceID = 0;

            if (HasHeaderFlag(PacketHeaderFlags.ResetConnection))
                //SessionID is duplicated with resetconnection header, indicates to drop session
				//What do we do if this... isn't the case?
				//Chalk it up to invalid packet and drop?
                if (SessionID == buffer.GetLEUInt(ref offset))
                {
                    CancelSession = true;
                    return;
                }
				
				
			if (HasHeaderFlag(PacketHeaderFlags.NewInstance))
                NewInstance = true;

            //Else?????

            //Check if it is a transfer packet
            if (!(TargetEndPoint == 0xFFFF))
                //Not Transfer packet, Validate CRC Checksum for packet
                CRCChecksum = buffer.Slice((buffer.Length - 4), 4).Span.SequenceEqual(CRC.calculateCRC(buffer.Slice(0, buffer.Length - 4).Span));

            else
                //Eventually do transfers here some how
                return;

            //Read Bundle Type, needs abit of a work around....
            bundleFlags = (PacketBundleFlags)buffer.GetByte(ref offset);

            ClientBundleNumber = buffer.GetLEUShort(ref offset);


            if (HasBundleFlag(PacketBundleFlags.NewProcessReport) || HasBundleFlag(PacketBundleFlags.ProcessMessageAndReport) ||
                HasBundleFlag(PacketBundleFlags.ProcessReport) || HasBundleFlag(PacketBundleFlags.ProcessAll))
            {
                ClientBundleAck = buffer.GetLEUShort(ref offset);
                ClientMessageAck = buffer.GetLEUShort(ref offset);
                RDPReport = true;
            }
			if (HasBundleFlag(PacketBundleFlags.NewProcessMessages) || HasBundleFlag(PacketBundleFlags.ProcessMessageAndReport) ||
                HasBundleFlag(PacketBundleFlags.ProcessMessages) || HasBundleFlag(PacketBundleFlags.ProcessAll))
				ProcessMessage = true;
        }

        public bool HasHeaderFlag(PacketHeaderFlags HeaderFlags) { return (HeaderFlags & headerFlags) == HeaderFlags; }

        public bool HasBundleFlag(PacketBundleFlags BundleFlags) { return (BundleFlags & bundleFlags) == BundleFlags; }
    }
}
