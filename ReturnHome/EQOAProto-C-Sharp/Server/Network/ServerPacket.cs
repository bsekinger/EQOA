using System;
using System.IO;

using ReturnHome.Utilities;

namespace ReturnHome.Server.Network
{
    public class ServerPacket : Packet
    {
        // Can still add packet header to this total
        public static int MaxPacketSize { get; } = 1024;
        public bool NeedAck { get; }

        /// <summary>
        /// Initializes Pipe stuff
        /// </summary>
		public MemoryStream Data { get; private set; }
        public BinaryWriter DataWriter { get; private set; }

        public int currentSize;


        public ServerPacket(int initialCapacity = 1050)
        {
            if (DataWriter == null)
            {
                Data = new MemoryStream(1024);
                DataWriter = new BinaryWriter(Data);
            }
        }

        public byte[] CreateReadyToSendPacket(Session session)
        {
            //Write our ServerID
            DataWriter.Write(session.Network.ServerId);

            //Write ClientID
            DataWriter.Write(session.Network.ClientId);

            //Write our packet header
            CreatePacketHeader(session);

            //Write instance ID
            DataWriter.Write(session.SessionID);

            //Write objectID if applicable
            if (session.InstanceID != 0)
                DataWriter.Write(Utility_Funcs.Technique(session.InstanceID));

            //Write bundle header
            WriteBundleType(session);

            //Write session ack if applicable
            if (session.Network.sendSessionAck)
                DataWriter.Write(session.SessionID);
                session.Network.sendSessionAck = false;

            //write packet#
            DataWriter.Write(session.Network.ConnectionData.PacketSequence++);

            //write bundle ack if applicable
            if (session.Network.sendAck)
            {
                DataWriter.Write(session.Network.lastReceivedPacketSequence);
                DataWriter.Write(session.Network.lastReceivedMessageSequence);
                session.Network.sendAck = false;
            }

            foreach (PacketMessage message in Messages)
                DataWriter.Write(message.Data.Span);

            DataWriter.Write(CRC.calculateCRC(Data.GetBuffer().AsSpan(0, (int)Data.Length)));
            byte[] _buff = Data.GetBuffer();
            return _buff[0..((int)Data.Length)];
        }

        private void CreatePacketHeader(Session session)
        {
            //First get packet size + 1 for bundle header byte
            uint value = 3;

            foreach (PacketMessage message in Messages)
                value += (uint)message.Data.Length;

            if (session.Network.sendAck)
                value += 4;

            if (session.Network.sendSessionAck)
                value += 4;

            //Get the packet bundle information
            //If session is not approved, and server is session initiator
            if (session.didServerInitiate && !session.Network.sessionApproved)
                value |= 0x080000;

            //Maybe eventually add? To cancel a session
            //if (session.Terminate)
            //    value |= 0x010000;

            if (session.hasInstance)
                value |= 0x002000;

            if (session.didServerInitiate)
                value |= 0x004000;

            else
                value |= 0x001000;

            DataWriter.Write(Utility_Funcs.Pack(value));
        }

        private void WriteBundleType(Session session)
        {
            byte[] segBody = new byte[1];

            //Always has this
            segBody[0] |= 0x20;

            //Locate a good location to identify us to trigger sending an rdp report
            //0xFB/0xF9 types?
            if (session.Network.sendAck)
                segBody[0] |= 0x03;
                //Need to include the rdpreport on packet before setting to false
                //session.Network.sendAck = false;

            //For receiving 0x40 unreliable object update from client, won't need for now
            //if (MySession.Channel40Ack)
            //{
            //segBody[0] |= 0x10;
            //}

            //When we ack client's new session, we use this. Used twice on an average connection...
            if (session.Network.sendSessionAck)
                segBody[0] |= 0x40;

            DataWriter.Write(segBody);
        }
    }
}
