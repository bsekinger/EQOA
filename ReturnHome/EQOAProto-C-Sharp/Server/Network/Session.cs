using System.Net;

namespace ReturnHome.Server.Network
{
    public class Session
    {
        public ushort ClientBundle = 0;
        public ushort ClientBundleAck = 0;
        public ushort ClientMessage = 0;
        public ushort ClientMessageAck = 0;
        public ushort ServerBundle = 0;
        public ushort ServerBundleAck = 0;
        public ushort ServerMessage = 0;
        public ushort ServerMessageAck = 0;

        public IPEndPoint EndPoint { get; }
        public ushort ClientEndPoint;
        public NetworkSession Network { get; set; }
        public uint InstanceID { get; }
        //public uint GameEventSequence { get; set; }

        //public SessionState State { get; set; }

        public uint AccountID { get; private set; }
        public string Username { get; private set; }

        public Session(ServerListener connectionListener, IPEndPoint endPoint, uint instanceID, ushort clientId, ushort serverId)
        {
            EndPoint = endPoint;
            InstanceID = instanceID;
            Network = new NetworkSession(this, connectionListener, clientId, serverId);
        }

        public void ProcessPacket(ClientPacket packet)
        {
			//Consider adding state eventually? This could help identify world server, character select and in game/in world
            //if (!CheckState(packet))
                //return;

            Network.ProcessPacket(packet);
        }

        /*
		public void Terminate(SessionTerminationReason reason, GameMessage message = null, ServerPacket packet = null, string extraReason = "")
        {
            // TODO: graceful SessionTerminationReason.AccountBooted handling

            if (packet != null)
            {
                Network.EnqueueSend(packet);
            }
			
            if (message != null)
            {
                Network.EnqueueSend(message);
            }
			
            PendingTermination = new SessionTerminationDetails()
            {
                ExtraReason = extraReason,
                Reason = reason
            };
        }
        */
    }
}
