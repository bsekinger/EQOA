using System;
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
        public bool CRCChecksum { get; set; } = false;
        public bool RDPReport { get; set; } = false;
        public bool CancelSession { get; set; } = false;
        public ushort BundleSize { get; set; }
        public uint SessionID { get; set; }
        public uint InstanceID { get; set; }
        public ushort ClientBundleNumber { get; set; }
        public ushort ClientBundleAck { get; set; }
        public ushort ClientMessageAck { get; set; } 

        public void Unpack(BinaryReader buffer, byte[] buffer2)
        {
            ClientEndPoint = buffer.ReadUInt16();
            TargetEndPoint = buffer.ReadUInt16();
            HeaderData = (uint)buffer.Read7BitEncodedInt();
            BundleSize = (ushort)(HeaderData & 0x7FF);
            headerFlags = (PacketHeaderFlags)(HeaderData - BundleSize);

            //Verify packet has instance in header
            if (HasHeaderFlag(PacketHeaderFlags.HasInstance))
                SessionID = buffer.ReadUInt32();

            //if Client is "remote", means it is not "master" anymore and an additional pack value to read which ties into character instanceID
            if (HasHeaderFlag(PacketHeaderFlags.IsRemote))
                InstanceID = (uint)Utility_Funcs.DoubleUnpack(buffer);

            else
                InstanceID = 0;

            if (HasHeaderFlag(PacketHeaderFlags.ResetConnection))
                //SessionID is duplicated with resetconnection header, indicates to drop session
                if (SessionID == buffer.ReadUInt32())
                {
                    CancelSession = true;
                    return;

                }



            //Check if it is a transfer packet
            if (!(TargetEndPoint == 0xFFFF))
                //Not Transfer packet, Validate CRC Checksum for packet
                CRCChecksum = buffer2[(buffer2.Length - 4)..buffer2.Length].SequenceEqual(CRC.calculateCRC(buffer2[0..(buffer2.Length - 4)].AsSpan()));

            else
                //Eventually do transfers here some how
                return;

            //Read Bundle Type, needs abit of a work around....
            bundleFlags = (PacketBundleFlags)buffer.ReadByte();

            ClientBundleNumber = buffer.ReadUInt16();


            if (HasBundleFlag(PacketBundleFlags.NewProcessReport) || HasBundleFlag(PacketBundleFlags.ProcessMessageAndReport) ||
                HasBundleFlag(PacketBundleFlags.ProcessReport) || HasBundleFlag(PacketBundleFlags.ProcessAll))
            {
                ClientBundleAck = buffer.ReadUInt16();
                ClientMessageAck = buffer.ReadUInt16();
                RDPReport = true;
            }
        }

        public bool HasHeaderFlag(PacketHeaderFlags HeaderFlags) { return (HeaderFlags & headerFlags) == HeaderFlags; }

        public bool HasBundleFlag(PacketBundleFlags BundleFlags) { return bundleFlags == BundleFlags; }
    }
}
