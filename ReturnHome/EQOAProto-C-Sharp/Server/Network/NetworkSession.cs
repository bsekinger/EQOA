using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Timers;

using ReturnHome.Server.Network.Enum;
using ReturnHome.Server.Network.GameMessages;
using ReturnHome.Server.Network.Handlers;
//using ReturnHome.Server.Network.Handlers;
using ReturnHome.Server.Network.Managers;

namespace ReturnHome.Server.Network
{
    public class NetworkSession
    {
        private const int minimumTimeBetweenBundles = 5; // 5ms
        private const int timeBetweenTimeSync = 20000; // 20s
        //private const int timeBetweenAck = 300; // 300ms

        private readonly Session session;
        private readonly ServerListener connectionListener;

        private readonly Object[] currentBundleLocks = new Object[(int)GameMessageGroup.QueueMax];
        private readonly NetworkBundle[] currentBundles = new NetworkBundle[(int)GameMessageGroup.QueueMax];

        private ConcurrentDictionary<ushort, ClientMessage> outOfOrderMessages = new ConcurrentDictionary<ushort, ClientMessage>();

        private DateTime nextSend = DateTime.UtcNow;

        // Resync will be started after ConnectResponse, and should immediately be sent then, so no delay here.
        // Fun fact: even though we send the server time in the ConnectRequest, client doesn't seem to use it?  Therefore we must TimeSync early so client doesn't see a skew when we send it later.
        public bool sendResync;
        private DateTime nextResync = DateTime.UtcNow;

        public readonly SessionConnectionData ConnectionData = new SessionConnectionData();

        //If an Ack is needed to client
        public bool sendAck = false;

        //Ack's client session, or makes sure client ack's ours.
        //Probably need checks to verify client has ack'd a session we created before continuing
        public bool sendSessionAck = false;
        public bool sessionApproved = false;

        //private DateTime nextAck = DateTime.UtcNow.AddMilliseconds(timeBetweenAck);

        public ushort lastReceivedPacketSequence = 0;
        public ushort lastReceivedMessageSequence = 0;

        /// <summary>
        /// This is referenced from many threads:<para />
        /// ConnectionListener.OnDataReceieve()->Session.HandlePacket()->This.HandlePacket(packet), This path can come from any client or other thinkable object.<para />
        /// WorldManager.UpdateWorld()->Session.Update(lastTick)->This.Update(lastTick)
        /// </summary>
        private readonly ConcurrentDictionary<ushort /*seq*/, ServerMessage> cachedMessages = new ConcurrentDictionary<ushort /*seq*/, ServerMessage>();

        private static readonly TimeSpan cachedPacketPruneInterval = TimeSpan.FromSeconds(5);
        private DateTime lastCachedPacketPruneTime;
        /// <summary>
        /// Number of seconds to retain cachedPackets
        /// </summary>
        private const int cachedPacketRetentionTime = 120;

        /// <summary>
        /// This is referenced by multiple thread:<para />
        /// [ConnectionListener Thread + 0] WorldManager.ProcessPacket()->SendLoginRequestReject()<para />
        /// [ConnectionListener Thread + 0] WorldManager.ProcessPacket()->Session.ProcessPacket()->NetworkSession.ProcessPacket()->DoRequestForRetransmission()<para />
        /// [ConnectionListener Thread + 1] WorldManager.ProcessPacket()->Session.ProcessPacket()->NetworkSession.ProcessPacket()-> ... AuthenticationHandler<para />
        /// [World Manager Thread] WorldManager.UpdateWorld()->Session.Update(lastTick)->This.Update(lastTick)<para />
        /// </summary>
        private readonly ConcurrentQueue<ServerPacket> packetQueue = new ConcurrentQueue<ServerPacket>();


        /// <summary>
        /// Stores the tick value for the when an active session will timeout. If this value is in the past, the session is dead/inactive.
        /// </summary>
        public long TimeoutTick { get; set; }

        public ushort ClientId { get; }
        public ushort ServerId { get; }

        public NetworkSession(Session session, ServerListener connectionListener, ushort clientId, ushort serverId)
        {
            this.session = session;
            this.connectionListener = connectionListener;

            ClientId = clientId;
            ServerId = serverId;

            // New network auth session timeouts will always be low.
            //For now hardcode 30 seconds, once we enter world it needs to be like... 2 seconds to ping clients, maybe 60 seconds to disconnect
            TimeoutTick = DateTime.UtcNow.AddSeconds(30000).Ticks;

            for (int i = 0; i < currentBundles.Length; i++)
            {
                currentBundleLocks[i] = new object();
                currentBundles[i] = new NetworkBundle();
            }
        }

        
        /// <summary>
        /// Enequeues a GameMessage for sending to this client.
        /// This may be called from many threads.
        /// </summary>
        /// <param name="messages">One or more GameMessages to send</param>
        public void EnqueueSend(params GameMessage[] messages)
        {
            if (isReleased) // Session has been removed
                return;

            foreach (var message in messages)
            {
                var grp = message.Group;
                var currentBundleLock = currentBundleLocks[(int)grp];
                lock (currentBundleLock)
                {
                    var currentBundle = currentBundles[(int)grp];
                    //packetLog.DebugFormat("[{0}] Enqueuing Message {1}", session.LoggingIdentifier, message.Opcode);
                    currentBundle.Enqueue(message);
                }
            }
        }

        /// <summary>
        /// Enqueues a ServerPacket for sending to this client.
        /// Currently this is only used publicly once during login.  If that changes it's thread safety should be re
        /// </summary>
        /// <param name="packets"></param>
        public void EnqueueSend(params ServerPacket[] packets)
        {
            if (isReleased) // Session has been removed
                return;

            foreach (var packet in packets)
            {
                //packetLog.DebugFormat("[{0}] Enqueuing Packet {1}", session.LoggingIdentifier, packet.GetHashCode());
                packetQueue.Enqueue(packet);
            }
        }
        
        /// <summary>
        /// Prunes the cachedPackets dictionary
        /// Checks if we should send the current bundle and then flushes all pending packets.
        /// </summary>
        public void Update()
        {
            if (isReleased) // Session has been removed
                return;

            if (DateTime.UtcNow - lastCachedPacketPruneTime > cachedPacketPruneInterval)
            {
                //Console.WriteLine("Pruning packets");
                //PruneCachedPackets();
            }

            for (int i = 0; i < currentBundles.Length; i++)
            {
                NetworkBundle bundleToSend = null;

                var group = (GameMessageGroup)i;

                var currentBundleLock = currentBundleLocks[i];
                lock (currentBundleLock)
                {
                    var currentBundle = currentBundles[i];

                    if (group == GameMessageGroup.InvalidQueue)
                    {

                        if (currentBundle.NeedsSending && DateTime.UtcNow >= nextSend)
                        {
                            //packetLog.DebugFormat("[{0}] Swapping bundle", session.LoggingIdentifier);
                            // Swap out bundle so we can process it
                            bundleToSend = currentBundle;
                            currentBundles[i] = new NetworkBundle();
                        }
                    }

                    else
                    {
                        if (currentBundle.NeedsSending && DateTime.UtcNow >= nextSend)
                        {
                            //packetLog.DebugFormat("[{0}] Swapping bundle", session.LoggingIdentifier);
                            // Swap out bundle so we can process it
                            bundleToSend = currentBundle;
                            currentBundles[i] = new NetworkBundle();
                        }
                    }
                }

                // Send our bundle if we have one
                // We should be able to execute this outside the lock as Sending is single threaded
                // and all future writes from other threads will go to the new bundle
                if (bundleToSend != null)
                {
                    Console.WriteLine("Sending bundle...");
                    SendBundle(bundleToSend, group);
                    nextSend = DateTime.UtcNow.AddMilliseconds(minimumTimeBetweenBundles);
                }
            }

            FlushPackets();
        }

        /*
        private void PruneCachedPackets()
        {
            lastCachedPacketPruneTime = DateTime.UtcNow;

            var currentTime = (ushort)Timer.PortalYearTicks;

            // Make sure our comparison still works when ushort wraps every 18.2 hours.
            var removalList = cachedPackets.Values.Where(x => (currentTime >= x.EQOAHeader.Time ? currentTime : currentTime + ushort.MaxValue) - x.Header.Time > cachedPacketRetentionTime);

            foreach (var packet in removalList)
                cachedPackets.TryRemove(packet.Header.Sequence, out _);
        }
        */

        // This is called from ConnectionListener.OnDataReceieve()->Session.ProcessPacket()->This
        /// <summary>
        /// Processes an incoming packet from a client.
        /// </summary>
        /// <param name="packet">The ClientPacket to process.</param>
        public void ProcessPacket(ClientPacket packet)
        {
			//Develop a way to see if session has been removed/disconnected
            if (isReleased) // Session has been removed
                return;

            //packetLog.DebugFormat("[{0}] Processing packet {1}", session.LoggingIdentifier, packet.Header.Sequence);
            //NetworkStatistics.C2S_Packets_Aggregate_Increment();

            if (packet.Header.HasHeaderFlag(PacketHeaderFlags.ResetConnection))
            {
                session.Terminate(SessionTerminationReason.PacketHeaderDisconnect);
                return;
            }
			
			//If packet packet# is less then expected packet#, let's drop it. 
			//Packet ordering is not gauranteed and messages that get recent have a new packet#, implying only messages are reliable
            var desiredSeq = lastReceivedPacketSequence + 1;
            if (packet.Header.ClientBundleNumber < desiredSeq)
            {
				//Delayed/lost packet, drop it
                return;
            }

            //Set our sequence# to our most recent, and accepted, packet.
            lastReceivedPacketSequence = packet.Header.ClientBundleNumber;

            // Processing stage
            // If we reach here, this is a packet we should proceed with processing.
            HandleOrderedPacket(packet);

            // Need to process messages in sequence
            //CheckOutOfOrderPackets();
        }

        
        const uint MaxNumNakSeqIds = 115; //464 + header = 484;  (464 - 4) / 4

        private DateTime LastRequestForRetransmitTime = DateTime.MinValue;

        /// <summary>
        /// Handles a packet<para />
        /// Packets at this stage are already verified, "half processed", and reordered
        /// </summary>
        /// <param name="packet">ClientPacket to handle</param>
        private void HandleOrderedPacket(ClientPacket packet)
        {
            //packetLog.DebugFormat("[{0}] Handling packet {1}", session.LoggingIdentifier, packet.Header.Sequence);

            // Received an rudp report, flush out old packet up to ack
            if (packet.Header.HasBundleFlag(PacketBundleFlags.NewProcessReport) || packet.Header.HasBundleFlag(PacketBundleFlags.ProcessMessageAndReport) ||
                packet.Header.HasBundleFlag(PacketBundleFlags.ProcessReport) || packet.Header.HasBundleFlag(PacketBundleFlags.ProcessAll))
            {
                AcknowledgeSequence(packet.Header.ClientMessageAck);
				
            }

            // This should be set on the first packet to the server indicating the client is logging in.
            // This is the start of a three-way handshake between the client and server (LoginRequest, ConnectRequest, ConnectResponse)
            // Note this would be sent to each server a client would connect too (Login and each world).
            // In our current implimenation we handle all roles in this one server.
            if (packet.Header.HasHeaderFlag(PacketHeaderFlags.NewInstance))
            {
                //Need to trigger session ack here
                //packetLog.Debug($"[{session.LoggingIdentifier}] LoginRequest");
                sendSessionAck = true;
                sendAck = true;
                AuthenticationHandler.HandleLoginRequest(packet, session); //Revisit this, authentication/verification of sent data needs to happen

                //Assuming this went well, lets assume we got the 2 correct messages?
                lastReceivedMessageSequence = 0x02;
                return;
            }

            // Process all messages out of the packet
            foreach (ClientPacketMessage message in packet.Messages)
            {
                //Check if Message is a ping request, if it is, process it and ack
                if (message.Header.MessageType == (byte)MessageType.PingMessage)
                {
                    if (message.Header.MessageNumber == lastReceivedMessageSequence + 1)
                    {
                        Console.WriteLine("Process Ping Request");
                        //Process Ping
                        lastReceivedMessageSequence++;
                    }
                    else
                        Console.WriteLine("Ping Request out of order");
                }
                else
                    ProcessMessage(message);

                if (message.Header.MessageType == (byte)MessageType.ReliableMessage || message.Header.MessageType == (byte)MessageType.PingMessage)
                    sendAck = true;
            }
                

            //After done processing messages, set session ack flag
            //Update the last received sequence.
            //if (packet.Header.Sequence != 0 && (packet.Header.Flags != PacketHeaderFlags.AckSequence))
                //lastReceivedPacketSequence = packet.Header.Sequence;
        }

        /// <summary>
        /// Processes a message, combining split messages as needed, then handling them
        /// </summary>
        /// <param name="message">ClientPacketMessage to process</param>
        private void ProcessMessage(ClientPacketMessage packetMessage)
        {
            //packetLog.DebugFormat("[{0}] Processing fragment {1}", session.LoggingIdentifier, fragment.Header.Sequence);

            ClientMessage message = null;
			
            message = new ClientMessage(packetMessage.Data);

            // If message is not null, we have a complete message to handle
			// when would it be null...?
            if (message != null)
            {
                // First check if this message is the next sequence, if it is not, add it to our outOfOrderMessages
                if (packetMessage.Header.MessageNumber == lastReceivedMessageSequence + 1)
                {
                    //packetLog.DebugFormat("[{0}] Handling fragment {1}", session.LoggingIdentifier, fragment.Header.Sequence);
                    HandleMessages(message);

                }
                else
                {
                    //packetLog.DebugFormat("[{0}] Fragment {1} is early, lastReceivedFragmentSequence = {2}", session.LoggingIdentifier, fragment.Header.Sequence, lastReceivedFragmentSequence);
                    outOfOrderMessages.TryAdd(packetMessage.Header.MessageNumber, message);
                }
            }
        }
        
        /// <summary>
        /// Handles a ClientMessage by calling InboundMessageManager
        /// </summary>
        /// <param name="message">ClientMessage to process</param>
        private void HandleMessages(ClientMessage message)
        {
            Console.WriteLine("Here");
            InboundMessageManager.HandleClientMessage(message, session);
            lastReceivedMessageSequence++;
        }
        
        /// <summary>
        /// Checks for received messages from Client that may be out of order, and if order has resumed, process them
        /// </summary>
        private void CheckOutOfOrderMessages()
        {
            while (outOfOrderMessages.TryRemove((ushort)(lastReceivedMessageSequence + 1), out var message))
            {
                HandleMessages(message);
            }
        }
        /*
        //private List<EchoStamp> EchoStamps = new List<EchoStamp>();

        private static int EchoLogInterval = 5;
        private static int EchoInterval = 10;
        private static float EchoThreshold = 2.0f;
        private static float DiffThreshold = 0.01f;

        private float lastClientTime;
        private DateTime lastServerTime;

        private double lastDiff;
        private int echoDiff;
        
        private void VerifyEcho(float clientTime)
        {
            if (session.Player == null || session.logOffRequestTime != DateTime.MinValue)
                return;

            var serverTime = DateTime.UtcNow;

            if (lastClientTime == 0)
            {
                lastClientTime = clientTime;
                lastServerTime = serverTime;
                return;
            }

            var serverTimeDiff = serverTime - lastServerTime;
            var clientTimeDiff = clientTime - lastClientTime;

            var diff = Math.Abs(serverTimeDiff.TotalSeconds - clientTimeDiff);

            if (diff > EchoThreshold && diff - lastDiff > DiffThreshold)
            {
                lastDiff = diff;
                echoDiff++;

                if (echoDiff >= EchoLogInterval)
                    log.Warn($"{session.Player.Name} - TimeSync error: {echoDiff} (diff: {diff})");

                if (echoDiff >= EchoInterval)
                {
                    log.Error($"{session.Player.Name} - disconnected for speedhacking");

                    var actionChain = new ActionChain();
                    actionChain.AddAction(session.Player, () =>
                    {
                        //session.Network.EnqueueSend(new GameMessageBootAccount(session));
                        session.Network.EnqueueSend(new GameMessageSystemChat($"TimeSync: client speed error", ChatMessageType.Broadcast));
                        session.LogOffPlayer();

                        echoDiff = 0;
                        lastDiff = 0;
                        lastClientTime = 0;

                    });
                    actionChain.EnqueueChain();
                }
            }
            else if (echoDiff > 0)
            {
                if (echoDiff > EchoLogInterval)
                    log.Warn($"{session.Player.Name} - Diff: {diff}");

                lastDiff = 0;
                echoDiff = 0;
            }
        }

        //is this special channel
        private void FlagEcho(float clientTime)
        {
            var currentBundleLock = currentBundleLocks[(int)GameMessageGroup.InvalidQueue];
            lock (currentBundleLock)
            {
                var currentBundle = currentBundles[(int)GameMessageGroup.InvalidQueue];

                // Debug.Assert(clientTime == -1f, "Multiple EchoRequests before Flush, potential issue with network logic!");
                currentBundle.ClientTime = clientTime;
                currentBundle.EncryptedChecksum = true;
            }
        }*/

        private void AcknowledgeSequence(ushort messageSequence)
        {
            //Remove stored messages here that the client ack's

            Console.WriteLine($"Checking for message to remove");

            var removalList = cachedMessages.Keys.Where(x => x < messageSequence);

            foreach (var key in removalList)
            {
                cachedMessages.TryRemove(key, out ServerMessage serverMessage);
                Console.WriteLine($"Removed Message #{serverMessage.Sequence}");
            }
        }

        /* this needs to eventually pack up and resend messages, they need to be resent every ~2 seconds untill ack received
        private bool Retransmit(ushort sequence)
        {
            if (cachedMessages.TryGetValue(sequence, out var cachedMessage))
            {

                //Need to construct a packet with resend messages I suppose?
                EnqueueSend(cachedMessage);

                return true;
            }

            if (cachedMessages.Count > 0)
            {
                // This is to catch a race condition between .Count and .Min() and .Max()
                try
                {
                    //log.Error($"Session {session.Network?.ClientId}\\{session.EndPoint} ({session.Account}:{session.Player?.Name}) retransmit requested packet {sequence} not in cache. Cache range {cachedPackets.Keys.Min()} - {cachedPackets.Keys.Max()}.");
                }
                catch
                {
                    //log.Error($"Session {session.Network?.ClientId}\\{session.EndPoint} ({session.Account}:{session.Player?.Name}) retransmit requested packet {sequence} not in cache. Cache is empty. Race condition threw exception.");
                }
            }
            else
                //log.Error($"Session {session.Network?.ClientId}\\{session.EndPoint} ({session.Account}:{session.Player?.Name}) retransmit requested packet {sequence} not in cache. Cache is empty.");

            return false;
        }*/
        
        private void FlushPackets()
        {
            while (packetQueue.TryDequeue(out var packet))
            {
                //Packet should be fully formed at this point... We need to cache messages for resend, not packets

                SendPacket(packet);
            }

            if (sendAck)
                SendPacket(new ServerPacket());
        }
        
        private void SendPacket(ServerPacket packet)
        {
            Console.WriteLine("Sending Packet...");
            SendPacketRaw(packet);
        }
        
        private void SendPacketRaw(ServerPacket packet)
        {

            
            try
            {
                var socket = connectionListener.Socket;

                byte[] buffer = packet.CreateReadyToSendPacket(session);

                try
                {
                    SocketAsyncEventArgs e = new();
                    e.SetBuffer(buffer);
                    e.RemoteEndPoint = session.EndPoint;

                    socket.SendToAsync(e);
                }

                catch (SocketException ex)
                {
                    // Unhandled Exception: System.Net.Sockets.SocketException: A message sent on a datagram socket was larger than the internal message buffer or some other network limit, or the buffer used to receive a datagram into was smaller than the datagram itself
                    // at System.Net.Sockets.Socket.UpdateStatusAfterSocketErrorAndThrowException(SocketError error, String callerName)
                    // at System.Net.Sockets.Socket.SendTo(Byte[] buffer, Int32 offset, Int32 size, SocketFlags socketFlags, EndPoint remoteEP)

                    var listenerEndpoint = (System.Net.IPEndPoint)socket.LocalEndPoint;
                    var sb = new StringBuilder();
                    sb.AppendLine(ex.ToString());
                    sb.AppendLine(String.Format("[{5}] Sending Packet (Len: {0}) [{1}:{2}=>{3}:{4}]", buffer.Length, listenerEndpoint.Address, listenerEndpoint.Port, session.EndPoint.Address, session.EndPoint.Port, session.Network.ClientId));
                    //log.Error(sb.ToString());

                    session.Terminate(SessionTerminationReason.SendToSocketException, null, null, ex.Message);
                }
            }

            finally
            {

            }
        }
        
        /// <summary>
        /// This function handles turning a bundle of messages (representing all messages accrued in a timeslice),
        /// into 1 or more packets, combining multiple messages into one packet or spliting large message across
        /// several packets as needed.
        /// </summary>
        /// <param name="bundle"></param>
        private void SendBundle(NetworkBundle bundle, GameMessageGroup group)
        {
            List<ServerMessage> messages = new List<ServerMessage>();

            // Pull all messages out and create MessageFragment objects
            while (bundle.HasMoreMessages)
            {
                var thisMessage = bundle.Dequeue();

                //If message is not FC type, it uses the sequence number ...Need to consider something to handle unreliables as they use their own message # per channel
                if (thisMessage.Messagetype != (byte)MessageType.UnreliableMessage)
                {
                    var message = new ServerMessage(thisMessage, ConnectionData.MessageSequence++);
                    //Store resend Messages here since these are not FC types?
                    Console.WriteLine($"Storing message {message.Sequence}");
                    cachedMessages.TryAdd(message.Sequence, message);
                    messages.Add(message);
                }

                //If message is FC type, does not use message #s
                else
                {
                    var message = new ServerMessage(thisMessage, 0);
                    messages.Add(message);
                }
            }

            //packetLog.DebugFormat("[{0}] Bundle Fragment Count: {1}", session.LoggingIdentifier, fragments.Count);

            // Loop through while we have messages
            while (messages.Count > 0)
            {
                ServerPacket packet = new ServerPacket();

                int availableSpace = ServerPacket.MaxPacketSize;

                // Pull first message and see if it is a large one
                var firstMessage = messages.FirstOrDefault();
                if (firstMessage != null)
                {
                    // If a large message send only this one, filling the whole packet
                    if (firstMessage.DataRemaining >= availableSpace)
                    {
                        //packetLog.DebugFormat("[{0}] Sending large fragment", session.LoggingIdentifier);
                        ServerPacketMessage spf = firstMessage.GetNextFragment();
                        packet.Messages.Add(spf);
                        availableSpace -= spf.Length;
                        if (firstMessage.DataRemaining <= 0)
                            messages.Remove(firstMessage);
                    }

                    // Create a list to remove completed messages after iterator
                    List<ServerMessage> removeList = new List<ServerMessage>();

                    foreach (ServerMessage message in messages)
                    {
                        bool fragmentSkipped = false;

                        // Is this a large message and does it have a tail that needs sending?
                        if (!message.TailSent && availableSpace >= message.TailSize)
                        {
                            //packetLog.DebugFormat("[{0}] Sending tail fragment", session.LoggingIdentifier);
                            ServerPacketMessage spf = message.GetTailFragment();
                            packet.Messages.Add(spf);
                            availableSpace -= spf.Length;
                        }

                        // Otherwise will this message fit in the remaining space?
                        else if (availableSpace >= message.NextSize)
                        {
                            //packetLog.DebugFormat("[{0}] Sending small message", session.LoggingIdentifier);
                            ServerPacketMessage spf = message.GetNextFragment();
                            packet.Messages.Add(spf);
                            availableSpace -= spf.Length;
                        }

                        else
                            fragmentSkipped = true;

                        // If message is out of data, set to remove it
                        if (message.DataRemaining <= 0)
                            removeList.Add(message);

                        // UIQueue messages must go out in order. Otherwise, you might see an NPC's tells in an order that doesn't match their defined emotes.
                        if (fragmentSkipped && group == GameMessageGroup.UIQueue)
                            break;
                    }

                    // Remove all completed messages
                    messages.RemoveAll(x => removeList.Contains(x));
                }

                //Always writemessage header information to server packet
                EnqueueSend(packet);
            }
        }
        
        private bool isReleased;

        /// <summary>
        /// This will empty out arrays, collections and dictionaries, and mark the object as released.
        /// Any further work assigned to this object will be ignored.
        /// </summary>
        public void ReleaseResources()
        {
            isReleased = true;

            for (int i = 0; i < currentBundles.Length; i++)
                currentBundles[i] = null;

            //partialFragments.Clear();
            outOfOrderMessages.Clear();

            cachedMessages.Clear();

            packetQueue.Clear();
        }
    }
}
