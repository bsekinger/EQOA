using System.Threading;
using System.Net;
using System;
using System.Linq;

using ReturnHome.Utilities;
using ReturnHome.Server.Network;
using ReturnHome.Server.Entity.Actions;
using ReturnHome.Server.Network.Enum;
using System.Threading.Tasks;

namespace ReturnHome.Server.Network.Managers
{
    public static class NetworkManager
    {
        // Hard coded server Id for now, consider changing to allow multiple servers/instances/processes
        public const ushort ServerID = 0x73B0;

        //Consider a session timeout eventually

        private static readonly ReaderWriterLockSlim sessionLock = new ReaderWriterLockSlim();

        ///This is our sessionList.
        public static ConcurrentHashSet<Session> SessionHash = new ConcurrentHashSet<Session>();

        /// <summary>
        /// Handles ClientMessages in InboundMessageManager
        /// </summary>
        public static readonly ActionQueue InboundMessageQueue = new ActionQueue();

        public static void ProcessPacket(ServerListener connectionListener, ClientPacket packet, IPEndPoint endPoint)
        {
            Session session;
            //ServerPerformanceMonitor.RestartEvent(ServerPerformanceMonitor.MonitorType.ProcessPacket_0);
            if (packet.Header.HasHeaderFlag(PacketHeaderFlags.NewInstance))
            {
                //packetLog.Debug($"{packet}, {endPoint}");
                //hardcoded for now
                if (GetAuthenticatedSessionCount() > 2000)
                {
                    //log.InfoFormat("Login Request from {0} rejected. Server full.", endPoint);
                    //Just let packet drop, no way to "reject" afaik
                }

                else
                {
                    //log.DebugFormat("Login Request from {0}", endPoint);

                    //hardcode to true for now
                    var ipAllowsUnlimited = true;
                    if (ipAllowsUnlimited)
                    {
                        session = new Session(connectionListener, endPoint, packet.Header.SessionID, packet.Header.InstanceID, packet.Header.ClientEndPoint, ServerID, false);
						
						//Try to add session to List, if it fails drop the session/packet, or process if it passes
                        if (SessionHash.TryAdd(session))
                        {
							//Session successfully added, process packet
                            session.ProcessPacket(packet);
                        }
						
						//If Session fails to create, just drop the packet
						session = null;
					}
                }
            }
			
			//Packet did not declare new instance request, so see if it exists
            else
            {
				//Find session, if it returns true, outputs session
                if (findSession(endPoint, packet.Header.InstanceID, out session))
                {
					//Checks if IP/Port matches expected session to incoming packet
                    if (session.EndPoint.Equals(endPoint))
                        session.ProcessPacket(packet);

                    else
                    {
                        //Somehow got the wrong session? Def. Needs a log to notate this
                        session = null;
                        //log.DebugFormat("Session for Id {0} has IP {1} but packet has IP {2}", packet.Header.Id, session.EndPoint, endPoint);
                    }
                }

                else
                {
                    //log.DebugFormat("Unsolicited Packet from {0} with Id {1}", endPoint, packet.Header.Id);
                }
            }
            
            //ServerPerformanceMonitor.RegisterEventEnd(ServerPerformanceMonitor.MonitorType.ProcessPacket_0);
        }
        /*
        private static void SendLoginRequestReject(ConnectionListener connectionListener, IPEndPoint endPoint, CharacterError error)
        {
            var tempSession = new Session(connectionListener, endPoint, (ushort)(sessionMap.Length + 1), ServerId);

            SendLoginRequestReject(tempSession, error);
        }

        public static void SendLoginRequestReject(Session session, CharacterError error)
        {
            // First we must send the connect request response
            var connectRequest = new PacketOutboundConnectRequest(
                Timers.PortalYearTicks,
                session.Network.ConnectionData.ConnectionCookie,
                session.Network.ClientId,
                session.Network.ConnectionData.ServerSeed,
                session.Network.ConnectionData.ClientSeed);
            session.Network.ConnectionData.DiscardSeeds();
            session.Network.EnqueueSend(connectRequest);

            // Then we send the error
            session.SendCharacterError(error);

            session.Network.Update();
        }
        */
        public static int GetSessionCount()
        {
            return SessionHash.Count;
        }
        

        public static int GetAuthenticatedSessionCount()
        {
            return SessionHash.Count();
        }
        /*

        public static int GetUniqueSessionEndpointCount()
        {
            sessionLock.EnterReadLock();
            try
            {
                var ipAddresses = new HashSet<IPAddress>();

                foreach (var s in sessionMap)
                {
                    if (s != null)
                        ipAddresses.Add(s.EndPoint.Address);
                }

                return ipAddresses.Count;
            }
            finally
            {
                sessionLock.ExitReadLock();
            }
        }

        public static int GetSessionEndpointTotalByAddressCount(IPAddress address)
        {
            sessionLock.EnterReadLock();
            try
            {
                int result = 0;

                foreach (var s in sessionMap)
                {
                    if (s != null && s.EndPoint.Address.Equals(address))
                        result++;
                }

                return result;
            }
            finally
            {
                sessionLock.ExitReadLock();
            }
        }
        */

        /// <summary>
        /// Removes a session, network client and network endpoint from the various tracker objects.
        /// </summary>
        public static bool findSession(IPEndPoint ClientIPEndPoint, uint InstanceID, out Session actualSession)
        {
            foreach (Session ClientSession in SessionHash)
            {
                if (ClientSession.EndPoint.Equals(ClientIPEndPoint) && ClientSession.InstanceID == InstanceID)
                {
                    actualSession = ClientSession;
                    return true;
                }
            }

            //Need logging to indicate actual session was not found
            actualSession = default;
            return false;
        }

        
        /// <summary>
        /// Processes all inbound GameAction messages.<para />
        /// Dispatches all outgoing messages.<para />
        /// Removes dead sessions.
        /// </summary>
        public static int DoSessionWork()
        {
            int sessionCount = 0;

            sessionLock.EnterUpgradeableReadLock();
            try
            {
                // The session tick outbound processes pending actions and handles outgoing messages
                //ServerPerformanceMonitor.RestartEvent(ServerPerformanceMonitor.MonitorType.DoSessionWork_TickOutbound);
                Parallel.ForEach(SessionHash, s => s?.TickOutbound());
                //ServerPerformanceMonitor.RegisterEventEnd(ServerPerformanceMonitor.MonitorType.DoSessionWork_TickOutbound);

                //Temporarily disable to test
                /*
                // Removes sessions in the NetworkTimeout state, including sessions that have reached a timeout limit.
                //ServerPerformanceMonitor.RestartEvent(ServerPerformanceMonitor.MonitorType.DoSessionWork_RemoveSessions);
                foreach (var session in SessionHash.Where(k => !Equals(null, k)))
                {
                    if (session.PendingTermination != null && session.PendingTermination.TerminationStatus == SessionTerminationPhase.SessionWorkCompleted)
                    {
                        session.DropSession();
                        session.PendingTermination.TerminationStatus = SessionTerminationPhase.WorldManagerWorkCompleted;
                    }

                    sessionCount++;
                }
                //ServerPerformanceMonitor.RegisterEventEnd(ServerPerformanceMonitor.MonitorType.DoSessionWork_RemoveSessions);*/
            }
            finally
            {
                sessionLock.ExitUpgradeableReadLock();
            }
            return sessionCount;
        }

        public static void RemoveSession(Session session)
        {
            if (SessionHash.TryRemove(session))
                Console.WriteLine("Session Successfully removed");
            else
                Console.WriteLine("Session not removed???");
        }

        /*
        public static void DisconnectAllSessionsForShutdown()
        {
            foreach (var session in sessionMap)
            {
                session?.Terminate(SessionTerminationReason.ServerShuttingDown, new GameMessages.Messages.GameMessageCharacterError(CharacterError.ServerCrash1));
            }
        }
        */
    }
}
