using System;
using System.IO.Pipelines;

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
        public Pipe pipe;

        public int currentSize;


        public ServerPacket(int initialCapacity = 1050)
        {
            pipe = new Pipe();
        }

        public Memory<byte> CreateReadyToSendPacket(Session session)
        {
            //Write our ServerID
            pipe.Writer.WriteAsync(BitConverter.GetBytes(session.Network.ServerId));

            //Write ClientID
            pipe.Writer.WriteAsync(BitConverter.GetBytes(session.Network.ClientId));

            //Write our packet header
            CreatePacketHeader(session);

            //Write instance ID
            pipe.Writer.WriteAsync(BitConverter.GetBytes(session.SessionID));

            //Write objectID if applicable
            if (session.InstanceID != 0)
                pipe.Writer.WriteAsync(Utility_Funcs.Technique(session.InstanceID));

            //Write bundle header
            WriteBundleType(session);

            //Write session ack if applicable
            if (session.Network.sendSessionAck)
                pipe.Writer.WriteAsync(BitConverter.GetBytes(session.SessionID));

            //write packet#
            pipe.Writer.WriteAsync(BitConverter.GetBytes(session.Network.ConnectionData.PacketSequence));

            //write bundle ack if applicable
            if(session.Network.sendAck)
            {
                pipe.Writer.WriteAsync(BitConverter.GetBytes(session.Network.lastReceivedPacketSequence));
                pipe.Writer.WriteAsync(BitConverter.GetBytes(session.Network.lastReceivedMessageSequence));
                session.Network.sendAck = false;
            }

            foreach (PacketMessage message in Messages)
                pipe.Writer.WriteAsync(message.Data);

            pipe.Writer.Complete();

            if (pipe.Reader.TryRead(out ReadResult OurResult))
            {
                try
                {
                    //Need to add our crc here
                    Memory<byte> buffer = new(new byte[OurResult.Buffer.First.Length + 4]);
                    OurResult.Buffer.First.CopyTo(buffer);
                    CRC.calculateCRC(OurResult.Buffer.First.Span).CopyTo(buffer[(buffer.Length - 4)..buffer.Length]);
                    return buffer;
                }

                finally
                {
                    pipe.Reader.Complete();
                    pipe.Reset();
                }
            }
            return default;
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

            pipe.Writer.WriteAsync(Utility_Funcs.Pack(value));
        }

        private void WriteBundleType(Session session)
        {
            byte[] segBody = new byte[1];

            //Always has this
            segBody[0] |= 0x20;

            //Locate a good location to identify us to trigger sending an rdp report
            //0xFB/0xF9 types?
            if (session.Network.sendAck)
            {
                segBody[0] |= 0x03;
                //Need to include the rdpreport on packet before setting to false
                //session.Network.sendAck = false;
            }

            //For receiving 0x40 unreliable object update from client, won't need for now
            //if (MySession.Channel40Ack)
            //{
                //segBody[0] |= 0x10;
            //}

            //When we ack client's new session, we use this. Used twice on an average connection...
            if (session.Network.sendSessionAck)
            {
                segBody[0] |= 0x40;
            }

            pipe.Writer.WriteAsync(segBody);
        }
    }
}
