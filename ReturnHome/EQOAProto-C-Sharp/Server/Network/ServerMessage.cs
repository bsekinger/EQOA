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
            //packetLog.DebugFormat("Sequence {0}, count {1}, DataRemaining {2}", sequence, Count, DataRemaining);
        }

        public ServerPacketMessage GetTailFragment()
        {
            var index = (ushort)(Count - 1);
            TailSent = true;
            return CreateServerFragment(index);
        }

        public ServerPacketMessage GetNextFragment()
        {
            return CreateServerFragment(Index++);
        }

        private ServerPacketMessage CreateServerFragment(ushort index)
        {
            //packetLog.DebugFormat("Creating ServerFragment for index {0}", index);
            if (index >= Count)
                throw new ArgumentOutOfRangeException("index", index, "Passed index is greater then computed count");

            var position = index * PacketMessage.MaxMessageSize;
            if (position > DataLength)
                throw new ArgumentOutOfRangeException("index", index, "Passed index computes to invalid position size");

            if (DataRemaining <= 0)
                throw new InvalidOperationException("There is no data remaining");

            var dataToSend = DataLength - position;
            if (dataToSend > PacketMessage.MaxMessageSize)
                dataToSend = PacketMessage.MaxMessageSize;

            if (DataRemaining < dataToSend)
                throw new InvalidOperationException("More data to send then data remaining!");

            // Read data starting at position reading dataToSend bytes
            Message.Data.Seek(position, SeekOrigin.Begin);
            byte[] data = new byte[dataToSend];
            Message.Data.Read(data, 0, dataToSend);

            // Build ServerPacketFragment structure
            ServerPacketMessage fragment = new ServerPacketMessage(data);
            fragment.Header.MessageNumber = Sequence;
            fragment.Header.Count = Count;
            fragment.Header.Index = index;

            DataRemaining -= dataToSend;
            //packetLog.DebugFormat("Done creating ServerFragment for index {0}. After reading {1} DataRemaining {2}", index, dataToSend, DataRemaining);
            return fragment;
        }
    }
}
