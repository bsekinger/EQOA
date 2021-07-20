﻿using System;
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

        private ConcurrentDictionary<ushort, ClientPacket> outOfOrderPackets = new ConcurrentDictionary<ushort, ClientPacket>();
        //private ConcurrentDictionary<uint, MessageBuffer> partialFragments = new ConcurrentDictionary<uint, MessageBuffer>(); // May not be needed... Don't think client sends us split messages
        private ConcurrentDictionary<ushort, ClientMessage> outOfOrderMessages = new ConcurrentDictionary<ushort, ClientMessage>();

        private DateTime nextSend = DateTime.UtcNow;

        // Resync will be started after ConnectResponse, and should immediately be sent then, so no delay here.
        // Fun fact: even though we send the server time in the ConnectRequest, client doesn't seem to use it?  Therefore we must TimeSync early so client doesn't see a skew when we send it later.
        public bool sendResync;
        private DateTime nextResync = DateTime.UtcNow;

        public readonly SessionConnectionData ConnectionData = new SessionConnectionData();

        // Ack should be sent after a 300 millisecond delay, so start enabled with the delay.
        private bool sendAck = false;
        //private DateTime nextAck = DateTime.UtcNow.AddMilliseconds(timeBetweenAck);

        private ushort lastReceivedPacketSequence = 0;
        private ushort lastReceivedMessageSequence = 0;

        /// <summary>
        /// This is referenced from many threads:<para />
        /// ConnectionListener.OnDataReceieve()->Session.HandlePacket()->This.HandlePacket(packet), This path can come from any client or other thinkable object.<para />
        /// WorldManager.UpdateWorld()->Session.Update(lastTick)->This.Update(lastTick)
        /// </summary>
        private readonly ConcurrentDictionary<ushort /*seq*/, ServerPacket> cachedPackets = new ConcurrentDictionary<ushort /*seq*/, ServerPacket>();

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
			// Look into this
            TimeoutTick = 30000;

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
                //PruneCachedPackets();

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

            //FlushPackets();
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

            #region order-insensitive "half-processing"

            if (packet.Header.HasHeaderFlag(PacketHeaderFlags.ResetConnection))
            {
                //session.Terminate(SessionTerminationReason.PacketHeaderDisconnect);
                return;
            }

            // depending on the current session state:
            // Set the next timeout tick value, to compare against in the WorldManager
            // Sessions that have gone past the AuthLoginRequest step will stay active for a longer period of time (exposed via configuration) 
            // Sessions that are in the AuthLoginRequest will have a short timeout, as set in the AuthenticationHandler.DefaultAuthTimeout.
            // Example: Applications that check uptime will stay in the AuthLoginRequest state.
            //session.Network.TimeoutTick = (session.State == SessionState.AuthLoginRequest) ?
                //DateTime.UtcNow.AddSeconds(AuthenticationHandler.DefaultAuthTimeout).Ticks : // Default is 15s
                //DateTime.UtcNow.AddSeconds(NetworkManager.DefaultSessionTimeout).Ticks; // Default is 60s

            #endregion

            #region Reordering stage

            // Check if this packet's sequence is greater then the next one we should be getting.
            // If true we must store it to replay once we have caught up.
			/*
			CONSIDERATION: We may not want to store packets based on bundle # and tracking that way. 
			Bundle #'s appear disposable, as a new packet that is the exact same as a prior one, will have the next sequence #.
			May be better to store data like messages for review later if the sequence is out of order.
			*/
			
            var desiredSeq = lastReceivedPacketSequence + 1;
            if (packet.Header.ClientBundleNumber > desiredSeq)
            {
                //packetLog.DebugFormat("[{0}] Packet {1} received out of order", session.LoggingIdentifier, packet.Header.Sequence);

                if (!outOfOrderPackets.ContainsKey(packet.Header.ClientBundleNumber))
                    outOfOrderPackets.TryAdd(packet.Header.ClientBundleNumber, packet);

				//Have an idea of what this request looks like, but need to solidify that one of these days, probably a final step to a "most" stable ingame experience.
                //if (desiredSeq + 2 <= packet.Header.Sequence && DateTime.UtcNow - LastRequestForRetransmitTime > new TimeSpan(0, 0, 1))
                    //DoRequestForRetransmission(packet.Header.Sequence);

                return;
            }

            #endregion

            #region Final processing stage

            // Processing stage
            // If we reach here, this is a packet we should proceed with processing.
            HandleOrderedPacket(packet);

            // Process data now in sequence
            // Finally check if we have any out of order packets or fragments we need to process;
            CheckOutOfOrderPackets();

            #endregion
        }

        
        const uint MaxNumNakSeqIds = 115; //464 + header = 484;  (464 - 4) / 4
        /*
        /// <summary>
        /// request retransmission of lost sequences
        /// </summary>
        /// <param name="rcvdSeq">the sequence of the packet that was just received.</param>
        private void DoRequestForRetransmission(uint rcvdSeq)
        {
            var desiredSeq = lastReceivedPacketSequence + 1;
            List<uint> needSeq = new List<uint>();
            needSeq.Add(desiredSeq);
            uint bottom = desiredSeq + 1;

            uint seqIdCount = 1;
            for (uint a = bottom; a < rcvdSeq; a++)
            {
                if (!outOfOrderPackets.ContainsKey(a))
                {
                    needSeq.Add(a);
                    seqIdCount++;
                    if (seqIdCount >= MaxNumNakSeqIds)
                    {
                        break;
                    }
                }
            }

            ServerPacket reqPacket = new ServerPacket();
            byte[] reqData = new byte[4 + (needSeq.Count * 4)];
            MemoryStream msReqData = new MemoryStream(reqData, 0, reqData.Length, true, true);
            msReqData.Write(BitConverter.GetBytes((uint)needSeq.Count), 0, 4);
            needSeq.ForEach(k => msReqData.Write(BitConverter.GetBytes(k), 0, 4));
            reqPacket.Data = msReqData;
            reqPacket.Header.Flags = PacketHeaderFlags.RequestRetransmit;

            EnqueueSend(reqPacket);

            LastRequestForRetransmitTime = DateTime.UtcNow;
            packetLog.DebugFormat("[{0}] Requested retransmit of {1}", session.LoggingIdentifier, needSeq.Select(k => k.ToString()).Aggregate((a, b) => a + ", " + b));
            NetworkStatistics.S2C_RequestsForRetransmit_Aggregate_Increment();
        }
        */

        private DateTime LastRequestForRetransmitTime = DateTime.MinValue;

        /// <summary>
        /// Handles a packet<para />
        /// Packets at this stage are already verified, "half processed", and reordered
        /// </summary>
        /// <param name="packet">ClientPacket to handle</param>
        private void HandleOrderedPacket(ClientPacket packet)
        {
            //packetLog.DebugFormat("[{0}] Handling packet {1}", session.LoggingIdentifier, packet.Header.Sequence);

            // If we have an EchoRequest flag, we should flag to respond with an echo response on next send.
			//How would we handle a echo request, this seems to be an opcode on eqoa, keep an eye out for this in opcode/message processing
            /*if (packet.Header.HasFlag(PacketHeaderFlags.EchoRequest))
            {
                FlagEcho(packet.HeaderOptional.EchoRequestClientTime);
                VerifyEcho(packet.HeaderOptional.EchoRequestClientTime);
            }*/

            // Received an rudp report, flush out old packet up to ack
			/* 
			CONSIDERATION: We will not store packets, rather messages. 
			Upon receiving a client ack, we will filter through and remove messages to ack
			*/
			
            if (packet.Header.HasBundleFlag(PacketBundleFlags.NewProcessReport) || packet.Header.HasBundleFlag(PacketBundleFlags.ProcessMessageAndReport) ||
                packet.Header.HasBundleFlag(PacketBundleFlags.ProcessReport) || packet.Header.HasBundleFlag(PacketBundleFlags.ProcessAll))
            {

            }
            //AcknowledgeSequence(packet.HeaderOptional.AckSequence); //Revisit this eventually, need to incorporate Packets/messages being saved based on message #

            /* Don't think we really need this
            if (packet.Header.HasFlag(PacketHeaderFlags.TimeSync))
            {
                //packetLog.DebugFormat("[{0}] Incoming TimeSync TS: {1}", session.LoggingIdentifier, packet.HeaderOptional.TimeSynch);
                // Do something with this...
                // Based on network traces these are not 1:1.  Server seems to send them every 20 seconds per port.
                // Client seems to send them alternatingly every 2 or 4 seconds per port.
                // We will send this at a 20 second time interval.  I don't know what to do with these when we receive them at this point.
            }
			*/

            // This should be set on the first packet to the server indicating the client is logging in.
            // This is the start of a three-way handshake between the client and server (LoginRequest, ConnectRequest, ConnectResponse)
            // Note this would be sent to each server a client would connect too (Login and each world).
            // In our current implimenation we handle all roles in this one server.
            if (packet.Header.HasHeaderFlag(PacketHeaderFlags.NewInstance))
            {
                //Need to trigger session ack here
                //packetLog.Debug($"[{session.LoggingIdentifier}] LoginRequest");
                
                AuthenticationHandler.HandleLoginRequest(packet, session); //Revisit this, authentication/verification of sent data needs to happen
                return;
            }

            // Process all messages out of the packet
            foreach (ClientPacketMessage message in packet.Messages)
                ProcessMessage(message);

            //After done processing messages, set session ack flag
            // Update the last received sequence.
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

            // Check if this message is split
			/* Messages from client *shouldn't* be big enough to have split
            if (packetMessage.Header.Split)
            {
                // Packet is split
                //packetLog.DebugFormat("[{0}] Fragment {1} is split, this index {2} of {3} fragments", session.LoggingIdentifier, fragment.Header.Sequence, fragment.Header.Index, fragment.Header.Count);

                if (partialFragments.TryGetValue(packetMessage.Header.Sequence, out var buffer))
                {
                    // Existing buffer, add this to it and check if we are finally complete.
                    buffer.AddFragment(fragment);
                    packetLog.DebugFormat("[{0}] Added fragment {1} to existing buffer. Buffer at {2} of {3}", session.LoggingIdentifier, fragment.Header.Sequence, buffer.Count, buffer.TotalFragments);
                    if (buffer.Complete)
                    {
                        // The buffer is complete, so we can go ahead and handle
                        packetLog.DebugFormat("[{0}] Buffer {1} is complete", session.LoggingIdentifier, buffer.Sequence);
                        message = buffer.GetMessage();
                        MessageBuffer removed = null;
                        partialFragments.TryRemove(fragment.Header.Sequence, out removed);
                    }
                }
                else
                {
                    // No existing buffer, so add a new one for this fragment sequence.
                    packetLog.DebugFormat("[{0}] Creating new buffer {1} for this split fragment", session.LoggingIdentifier, fragment.Header.Sequence);
                    var newBuffer = new MessageBuffer(fragment.Header.Sequence, fragment.Header.Count);
                    newBuffer.AddFragment(fragment);

                    packetLog.DebugFormat("[{0}] Added fragment {1} to the new buffer. Buffer at {2} of {3}", session.LoggingIdentifier, fragment.Header.Sequence, newBuffer.Count, newBuffer.TotalFragments);
                    partialFragments.TryAdd(fragment.Header.Sequence, newBuffer);
                }
            }
			*/
			
            // Packet is not split, proceed with handling it.
            // packetLog.DebugFormat("[{0}] Fragment {1} is not split", session.LoggingIdentifier, fragment.Header.Sequence);
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
        /// Handles a ClientMessage by calling using InboundMessageManager
        /// </summary>
        /// <param name="message">ClientMessage to process</param>
        private void HandleMessages(ClientMessage message)
        {
            Console.WriteLine("Here");
            InboundMessageManager.HandleClientMessage(message, session);
            lastReceivedMessageSequence++;
        }

        
        /// <summary>
        /// Checks if we now have packets queued out of order which should be processed as the next sequence.
        /// </summary>
        private void CheckOutOfOrderPackets()
        {
            while (outOfOrderPackets.TryRemove((ushort)(lastReceivedPacketSequence + 1), out var packet))
            {
                //packetLog.DebugFormat("[{0}] Ready to handle out-of-order packet {1}", session.LoggingIdentifier, packet.Header.Sequence);
                HandleOrderedPacket(packet);
            }
        }
        
        /// <summary>
        /// Checks if we now have fragments queued out of order which should be handled as the next sequence.
        /// </summary>
        private void CheckOutOfOrderFragments()
        {
            while (outOfOrderMessages.TryRemove((ushort)(lastReceivedMessageSequence + 1), out var message))
            {
                //packetLog.DebugFormat("[{0}] Ready to handle out of order fragment {1}", session.LoggingIdentifier, lastReceivedMessageSequence + 1);
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
        }

        private void AcknowledgeSequence(uint sequence)
        {
            // TODO Sending Acks seems to cause some issues.  Needs further research.
            // if (!sendAck)
            //    sendAck = true;

            var removalList = cachedPackets.Keys.Where(x => x < sequence);

            foreach (var key in removalList)
                cachedPackets.TryRemove(key, out _);
        }

        private bool Retransmit(uint sequence)
        {
            if (cachedPackets.TryGetValue(sequence, out var cachedPacket))
            {
                packetLog.DebugFormat("[{0}] Retransmit {1}", session.LoggingIdentifier, sequence);

                if (!cachedPacket.Header.HasFlag(PacketHeaderFlags.Retransmission))
                    cachedPacket.Header.Flags |= PacketHeaderFlags.Retransmission;

                SendPacketRaw(cachedPacket);

                return true;
            }

            if (cachedPackets.Count > 0)
            {
                // This is to catch a race condition between .Count and .Min() and .Max()
                try
                {
                    log.Error($"Session {session.Network?.ClientId}\\{session.EndPoint} ({session.Account}:{session.Player?.Name}) retransmit requested packet {sequence} not in cache. Cache range {cachedPackets.Keys.Min()} - {cachedPackets.Keys.Max()}.");
                }
                catch
                {
                    log.Error($"Session {session.Network?.ClientId}\\{session.EndPoint} ({session.Account}:{session.Player?.Name}) retransmit requested packet {sequence} not in cache. Cache is empty. Race condition threw exception.");
                }
            }
            else
                log.Error($"Session {session.Network?.ClientId}\\{session.EndPoint} ({session.Account}:{session.Player?.Name}) retransmit requested packet {sequence} not in cache. Cache is empty.");

            return false;
        }*/
        /*
        private void FlushPackets()
        {
            while (packetQueue.TryDequeue(out var packet))
            {
                //packetLog.DebugFormat("[{0}] Flushing packets, count {1}", session.LoggingIdentifier, packetQueue.Count);

                if (packet.Header.HasFlag(PacketHeaderFlags.EncryptedChecksum) && ConnectionData.PacketSequence.CurrentValue == 0)
                    ConnectionData.PacketSequence = new Sequence.UIntSequence(1);

                bool isNak = packet.Header.Flags.HasFlag(PacketHeaderFlags.RequestRetransmit);

                // If we are only ACKing, then we don't seem to have to increment the sequence
                if (packet.Header.Flags == PacketHeaderFlags.AckSequence || isNak)
                    packet.Header.Sequence = ConnectionData.PacketSequence.CurrentValue;
                else
                    packet.Header.Sequence = ConnectionData.PacketSequence.NextValue;
                packet.Header.Id = ServerId;
                packet.Header.Iteration = 0x14;
                packet.Header.Time = (ushort)Timers.PortalYearTicks;

                if (packet.Header.Sequence >= 2u && !isNak)
                    cachedPackets.TryAdd(packet.Header.Sequence, packet);

                SendPacket(packet);
            }
        }
        
        private void SendPacket(ServerPacket packet)
        {
            //packetLog.DebugFormat("[{0}] Sending packet {1}", session.LoggingIdentifier, packet.GetHashCode());
            //NetworkStatistics.S2C_Packets_Aggregate_Increment();

            //SendPacketRaw(packet);
        }
        /*
        private void SendPacketRaw(ServerPacket packet)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent((int)(PacketHeader.HeaderSize + (packet.Data?.Length ?? 0) + (packet.Messages.Count * PacketMessage.)));

            try
            {
                var socket = connectionListener.Socket;

                packet.CreateReadyToSendPacket(buffer, out var size);

                //packetLog.Debug(packet.ToString());

                //if (packetLog.IsDebugEnabled)
                //{
                    //var listenerEndpoint = (System.Net.IPEndPoint)socket.LocalEndPoint;
                    //var sb = new StringBuilder();
                    //sb.AppendLine(String.Format("[{5}] Sending Packet (Len: {0}) [{1}:{2}=>{3}:{4}]", size, listenerEndpoint.Address, listenerEndpoint.Port, session.EndPoint.Address, session.EndPoint.Port, session.Network.ClientId));
                    //sb.AppendLine(buffer.BuildPacketString(0, size));
                    //packetLog.Debug(sb.ToString());
                //}

                try
                {
                    socket.SendTo(buffer, size, SocketFlags.None, session.EndPoint);
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
                ArrayPool<byte>.Shared.Return(buffer, true);
            }
        }*/
        
        /// <summary>
        /// This function handles turning a bundle of messages (representing all messages accrued in a timeslice),
        /// into 1 or more packets, combining multiple messages into one packet or spliting large message across
        /// several packets as needed.
        /// </summary>
        /// <param name="bundle"></param>
        private void SendBundle(NetworkBundle bundle, GameMessageGroup group)
        {
            //packetLog.DebugFormat("[{0}] Sending Bundle", session.LoggingIdentifier);

            bool writeOptionalHeaders = true;

            List<ServerMessage> fragments = new List<ServerMessage>();

            // Pull all messages out and create MessageFragment objects
            while (bundle.HasMoreMessages)
            {
                var message = bundle.Dequeue();

                var fragment = new ServerMessage(message, ConnectionData.MessageSequence++);
                fragments.Add(fragment);
            }

            //packetLog.DebugFormat("[{0}] Bundle Fragment Count: {1}", session.LoggingIdentifier, fragments.Count);

            // Loop through while we have fragements
            while (fragments.Count > 0 || writeOptionalHeaders)
            {
                ServerPacket packet = new ServerPacket();
                PacketHeader packetHeader = packet.Header;

                int availableSpace = ServerPacket.MaxPacketSize;

                // Pull first message and see if it is a large one
                var firstMessage = fragments.FirstOrDefault();
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
                            fragments.Remove(firstMessage);
                    }
                    // Create a list to remove completed messages after iterator
                    List<ServerMessage> removeList = new List<ServerMessage>();

                    foreach (ServerMessage fragment in fragments)
                    {
                        bool fragmentSkipped = false;

                        // Is this a large fragment and does it have a tail that needs sending?
                        if (!fragment.TailSent && availableSpace >= fragment.TailSize)
                        {
                            //packetLog.DebugFormat("[{0}] Sending tail fragment", session.LoggingIdentifier);
                            ServerPacketMessage spf = fragment.GetTailFragment();
                            packet.Messages.Add(spf);
                            availableSpace -= spf.Length;
                        }

                        // Otherwise will this message fit in the remaining space?
                        else if (availableSpace >= fragment.NextSize)
                        {
                            //packetLog.DebugFormat("[{0}] Sending small message", session.LoggingIdentifier);
                            ServerPacketMessage spf = fragment.GetNextFragment();
                            packet.Messages.Add(spf);
                            availableSpace -= spf.Length;
                        }

                        else
                            fragmentSkipped = true;

                        // If message is out of data, set to remove it
                        if (fragment.DataRemaining <= 0)
                            removeList.Add(fragment);

                        // UIQueue messages must go out in order. Otherwise, you might see an NPC's tells in an order that doesn't match their defined emotes.
                        if (fragmentSkipped && group == GameMessageGroup.UIQueue)
                            break;

                        // Remove all completed messages
                        fragments.RemoveAll(x => removeList.Contains(x));
                    }
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

            outOfOrderPackets.Clear();
            //partialFragments.Clear();
            outOfOrderMessages.Clear();

            cachedPackets.Clear();

            packetQueue.Clear();
        }
    }
}
