using System;
using System.IO;

using ReturnHome.Server.Network.GameMessages;

namespace ReturnHome.Server.Network
{
    internal class ServerMessage
    {
        //private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //private static readonly ILog packetLog = LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), "Packets");

        public GameMessage Message { get; private set; }
		
		private byte[] data;
		
        public ushort Sequence { get; set; }

        public ushort Index { get; set; }

        public ushort Count { get; set; }

        public int DataLength => (int)Message.Data.Length;

        public int DataRemaining { get; private set; }

        public int NextSize
        {
            get
            {
                var dataSize = DataRemaining;
                if (dataSize > PacketMessage.MaxMessageSize)
                    dataSize = PacketMessage.MaxMessageSize;
                return dataSize;
            }
        }

        public int TailSize => (DataLength % PacketMessage.MaxMessageSize);

        public bool TailSent { get; private set; }

        public ServerMessage(GameMessage message, ushort sequence)
        {
            Message = message;
            DataRemaining = DataLength;
            Sequence = sequence;
            Count = (ushort)(Math.Ceiling((double)DataLength / PacketMessage.MaxMessageSize));
            Console.WriteLine($"{Count} expected packets from message");
            Index = 0;
            if (Count == 1)
                TailSent = true;
            //packetLog.DebugFormat("Sequence {0}, Count {1}, DataRemaining {2}", sequence, Count, DataRemaining);
        }

        public ServerPacketMessage GetTailFragment()
        {
            var Index = (ushort)(Count - 1);
            TailSent = true;
            return CreateServerFragment(Index);
        }

        public ServerPacketMessage GetNextFragment()
        {
            return CreateServerFragment(Index++);
        }

        private ServerPacketMessage CreateServerFragment(ushort Index)
        {
            //packetLog.DebugFormat("Creating ServerFragment for Index {0}", Index);
            if (Index >= Count)
                throw new ArgumentOutOfRangeException("Index", Index, "Passed Index is greater then computed Count");

            var position = Index * PacketMessage.MaxMessageSize;
            if (position > DataLength)
                throw new ArgumentOutOfRangeException("Index", Index, "Passed Index computes to invalid position size");

            if (DataRemaining <= 0)
                throw new InvalidOperationException("There is no data remaining");

            var dataToSend = DataLength - position;
            if (dataToSend > PacketMessage.MaxMessageSize)
                dataToSend = PacketMessage.MaxMessageSize;

            if (DataRemaining < dataToSend)
                throw new InvalidOperationException("More data to send then data remaining!");

            // Read data starting at position reading dataToSend bytes
            Message.Data.Seek(position, SeekOrigin.Begin);
			
			ProcessMessageTypeAndHeader(dataToSend);

            // Build ServerPacketFragment structure
            ServerPacketMessage fragment = new ServerPacketMessage(data);
            fragment.Header.MessageNumber = Sequence;
            fragment.Header.Count = Count;
            fragment.Header.Index = Index;

            DataRemaining -= dataToSend;
            //packetLog.DebugFormat("Done creating ServerFragment for Index {0}. After reading {1} DataRemaining {2}", Index, dataToSend, DataRemaining);
            return fragment;
        }
		
		private void ProcessMessageTypeAndHeader(int dataToSend)
		{
			if(Message.Messagetype == (byte)MessageType.ReliableMessage)
			{
				ProcessFBType(dataToSend);
			}
			
			//This MessageType has no message #'s, fire and forget
			else if (Message.Messagetype == (byte)MessageType.UnreliableMessage)
			{
				ProcessFCType(dataToSend);
			}
			
			//F9 type, seems to be a "ping" response
			else if (Message.Messagetype == (byte)MessageType.PingMessage)
			{
				ProcessF9(dataToSend);
			}
			
			//Reserved for unreliable message types
			else
			{
				ProcessUnreliable(dataToSend);
			}
		}
		
		private void ProcessFBType(int dataToSend)
		{
			//Means there is a need for multiple packets for this message, and it is not the last packet of this message
			if(Index < Count)
			{
				//If message is greater then 255 bytes, prefix with FF
				if (dataToSend > 255)
				{
					//Check if this message needs to span multiple packets
					data = new byte[dataToSend + 6];
					data[0] = 0xFF;
					data[1] = 0xFB;
					data[2] = (byte)(dataToSend & 0x00FF);
					data[3] = (byte)(dataToSend & 0xFF);
					data[4] = (byte)(Sequence);
					data[5] = (byte)(Sequence >> 8);
					Message.Data.Read(data, 6, dataToSend);
				}
					
				else
				{
					data = new byte[dataToSend + 4];
					data[0] = 0xFB;
					data[1] = (byte)(dataToSend);
					data[2] = (byte)(Sequence);
					data[3] = (byte)(Sequence >> 8);
					Message.Data.Read(data,4, dataToSend);
				}
			}
			
			else
			{
				//If message is greater then 255 bytes, prefix with FF
				if (dataToSend > 255)
				{
					data = new byte[dataToSend + 6];
					data[0] = 0xFF;
					data[1] = 0xFA;
					data[2] = (byte)(dataToSend & 0x00FF);
					data[3] = (byte)(dataToSend & 0xFF);
					data[4] = (byte)(Sequence);
					data[5] = (byte)(Sequence >> 8);
					Message.Data.Read(data, 6, dataToSend);
				}
					
				else
				{
					data = new byte[dataToSend + 4];
					data[0] = 0xFB;
					data[1] = (byte)(dataToSend);
					data[2] = (byte)(Sequence);
					data[3] = (byte)(Sequence >> 8);
					Message.Data.Read(data,4, dataToSend);
				}
			}
		}
		
		private void ProcessFCType(int dataToSend)
		{
			//If message is greater then 255 bytes, prefix with FF
			if (dataToSend > 255)
			{
				data = new byte[dataToSend + 4];
				data[0] = 0xFF;
				data[1] = 0xFC;
				data[2] = (byte)(dataToSend & 0x00FF);
				data[3] = (byte)(dataToSend & 0xFF);
				Message.Data.Read(data, 4, dataToSend);
			}
					
			else
			{
				data = new byte[dataToSend + 2];
				data[0] = 0xFB;
				data[1] = (byte)(dataToSend);
				Message.Data.Read(data,2, dataToSend);
			}
		}
		
		private void ProcessF9(int dataToSend)
		{
			data = new byte[dataToSend + 4];
			data[0] = 0xF9;
			data[1] = (byte)(dataToSend);
			data[2] = (byte)(Sequence);
			data[3] = (byte)(Sequence >> 8);
			Message.Data.Read(data,4, dataToSend);
		}
		
		private void ProcessUnreliable(int dataToSend)
		{
			
		}
    }
}
