using System;
using System.Net;
using ReturnHome.Database.Models.Auth;
using ReturnHome.Entity.Enum;
using ReturnHome.Server.Managers;
using ReturnHome.Server.Network.Enum;
using ReturnHome.Server.Network.GameEvent.Events;
using ReturnHome.Server.Network.GameMessages;
using ReturnHome.Server.Network.Managers;

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

        public DateTime logOffRequestTime;
        public DateTime lastCharacterSelectPingReply;
        public AccessLevel AccessLevel { get; private set; }
        public IPEndPoint EndPoint { get; }
        public ushort ClientEndPoint;
        public NetworkSession Network { get; set; }
        public uint InstanceID { get; }
        public int SessionID { get; }

        public bool didServerInitiate { get; }
        public bool hasInstance { get; }


        public SessionState State { get; set; }

        public SessionTerminationDetails PendingTermination { get; set; } = null;

        public uint AccountID { get; private set; }
        public string Username { get; private set; }

        public Session(ServerListener connectionListener, IPEndPoint endPoint, int sessionID, uint instanceID, ushort clientId, ushort serverId, bool DidServerInitiate)
        {
            didServerInitiate = DidServerInitiate;
            SessionID = sessionID;
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

        public void SetAccount(uint accountId, string account, AccessLevel accountAccesslevel)
        {
            AccountID = accountId;
            Username = account;
            AccessLevel = accountAccesslevel;
        }

        public void DropSession()
        {
            if (PendingTermination == null || PendingTermination.TerminationStatus != SessionTerminationPhase.SessionWorkCompleted) return;

            if (PendingTermination.Reason != SessionTerminationReason.PongSentClosingConnection)
            {
                var reason = PendingTermination.Reason;
                string reas = (reason != SessionTerminationReason.None) ? $", Reason: {reason.GetDescription()}" : "";
                if (!string.IsNullOrWhiteSpace(PendingTermination.ExtraReason))
                {
                    reas = reas + ", " + PendingTermination.ExtraReason;
                }
                //if (WorldManager.WorldStatus == WorldManager.WorldStatusState.Open)
                    //continue;
                //log.Info($"Session {Network?.ClientId}\\{EndPoint} dropped. Account: {Account}, Player: {Player?.Name}{reas}");
                //else
                   // log.Debug($"Session {Network?.ClientId}\\{EndPoint} dropped. Account: {Account}, Player: {Player?.Name}{reas}");
            }

            //if (Player != null)
            //{
                //LogOffPlayer();

                // We don't want to set the player to null here. Because the player is still on the network, it may still enqueue work onto it's session.
                // Some network message objects will reference session.Player in their construction. If we set Player to null here, we'll throw exceptions in those cases.

                // At this point, if the player was on a landblock, they'll still exist on that landblock until the logout animation completes (~6s).
            //}

            NetworkManager.RemoveSession(this);

            // This is a temp fix to mark the Session.Network portion of the Session as released
            // What this means is that we will release any network related resources, as well as avoid taking on additional resources
            // In the future, we should set Network to null and funnel Network communication through Session, instead of accessing Session.Network directly.
            Network.ReleaseResources();
        }

        /// <summary>
        /// This will send outgoing packets as well as the final logoff message.
        /// </summary>
        public void TickOutbound()
        {
            // Check if the player has been booted
            if (PendingTermination != null)
            {
                if (PendingTermination.TerminationStatus == SessionTerminationPhase.Initialized)
                {
                    State = SessionState.TerminationStarted;
                    Network.Update(); // boot messages may need sending
                    if (DateTime.UtcNow.Ticks > PendingTermination.TerminationEndTicks)
                        PendingTermination.TerminationStatus = SessionTerminationPhase.SessionWorkCompleted;
                }
                return;
            }

            if (State == SessionState.TerminationStarted)
                return;

            // Checks if the session has stopped responding.
            if (DateTime.UtcNow.Ticks >= Network.TimeoutTick)
            {
                // The Session has reached a timeout.  Send the client the error disconnect signal, and then drop the session
                Terminate(SessionTerminationReason.NetworkTimeout);
                return;
            }

            Network.Update();

            // Live server seemed to take about 6 seconds. 4 seconds is nice because it has smooth animation, and saves the user 2 seconds every logoff
            // This could be made 0 for instant logoffs.
            if (logOffRequestTime != DateTime.MinValue && logOffRequestTime.AddSeconds(6) <= DateTime.UtcNow)
                //SendFinalLogOffMessages(); /Eventually use this to save the character to database.

            // This section deviates from known retail pcaps/behavior, but appears to be the least harmful way to work around something that seemingly didn't occur to players using ThwargLauncher connecting to retail servers.
            // In order to prevent the launcher from thinking the session is dead, we will send a Ping Response every 100 seconds, this will in effect make the client appear active to the launcher and allow players to create characters in peace.
            if (State == SessionState.AuthConnected) // TODO: why is this needed? Why didn't retail have this problem? Is this fuzzy memory?
            {
                if (lastCharacterSelectPingReply == DateTime.MinValue)
                    lastCharacterSelectPingReply = DateTime.UtcNow.AddSeconds(100);
                else if (DateTime.UtcNow > lastCharacterSelectPingReply)
                {
                    Network.EnqueueSend(new GameEventPingResponse(this));
                    lastCharacterSelectPingReply = DateTime.UtcNow.AddSeconds(100);
                }
            }
            else if (lastCharacterSelectPingReply != DateTime.MinValue)
                lastCharacterSelectPingReply = DateTime.MinValue;
        }

        public void Terminate(SessionTerminationReason reason, GameMessage message = null, ServerPacket packet = null, string extraReason = "")
        {
            // TODO: graceful SessionTerminationReason.AccountBooted handling

            if (packet != null)
            {
                //Network.EnqueueSend(packet);
            }
			
            if (message != null)
            {
                //Network.EnqueueSend(message);
            }
			
            PendingTermination = new SessionTerminationDetails()
            {
                ExtraReason = extraReason,
                Reason = reason
            };
        }
    }
}
