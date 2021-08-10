using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReturnHome.Database.Models.Auth;
using ReturnHome.Entity.Enum;
using ReturnHome.Server.Managers;
using ReturnHome.Server.Network.Enum;
using ReturnHome.Server.Network.GameMessages.Messages;
using ReturnHome.Server.Network.Packets;

namespace ReturnHome.Server.Network.Handlers
{
    public static class AuthenticationHandler
    {
        /// <summary>
        /// Seconds until an authentication request will timeout/expire.
        /// </summary>
        public const int DefaultAuthTimeout = 15;

        //private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //private static readonly ILog packetLog = LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), "Packets");

        public static void HandleLoginRequest(ClientPacket packet, Session session)
        {
            GameDiscVersion version = new GameDiscVersion(packet.Messages[0]);
            try
            {
                //Process GameDisc first
                if (!(version.pass))
                {
                    //Drop packet and fail if this fails
                }

                //Process Account verification
                PacketInboundLoginRequest loginRequest = new PacketInboundLoginRequest(packet.Messages[1]);
                session.Version = version.Version;

                if (loginRequest.AccountName.Length > 16 || !loginRequest.EQOACheck)
                {
                    //We would just not respond to client, it would handle terminating session and starting new one.
                    session.Terminate(SessionTerminationReason.AccountInformationInvalid);
                    return;
                }

                Task t = new Task(() => DoLogin(session, loginRequest));
                t.Start();
            }
            catch (Exception ex)
            {
                //log.ErrorFormat("Received LoginRequest from {0} that threw an exception.", session.EndPoint);
                //log.Error(ex);
            }
        }
        
        private static void DoLogin(Session session, PacketInboundLoginRequest loginRequest)
        {
            /*Do a bunch of checks against sent password and whats in database.
            After password decryption... something like*/
            if (PacketInboundLoginRequest.VerifyPassword(out Account account))
            {

                try
                {
                    //log.Debug($"new client connected: {loginRequest.Account}. setting session properties");
                    AccountSelectCallback(account, session, loginRequest);

                    Console.WriteLine("Account Verified");
                }
                catch (Exception ex)
                {
                    //log.Error("Error in HandleLoginRequest trying to find the account.", ex);
                    session.Terminate(SessionTerminationReason.AccountSelectCallbackException);
                }
            }
        }

        
        private static void AccountSelectCallback(Account account, Session session, PacketInboundLoginRequest loginRequest)
        {
            //packetLog.DebugFormat("ConnectRequest TS: {0}", Timers.PortalYearTicks);
            /*Assuming we passed, create our message

            //If account is banned, just ignore... someone would have to circumvent the account login to get to this point
            if (account.BanExpireTime.HasValue)
            {
                var now = DateTime.UtcNow;
                if (now < account.BanExpireTime.Value)
                {
                    var reason = account.BanReason;
                    session.Terminate(SessionTerminationReason.AccountBanned, new GameMessageAccountBanned(account.BanExpireTime.Value, $"{(reason != null ? $" - {reason}" : null)}"), null, reason);
                    return;
                }
                else
                {
                    account.UnBan();
                }
            }
            */
            var GameVersion = new GameDiscVersion(session.Version);
            session.Network.EnqueueSend(GameVersion);

            
            //account.UpdateLastLogin(session.EndPoint.Address);

            session.SetAccount(account.AccountId, account.AccountName, (AccessLevel)account.AccessLevel);
            session.State = SessionState.AuthConnectResponse;
        }
        /*
        public static void HandleConnectResponse(Session session)
        {
            if (WorldManager.WorldStatus == WorldManager.WorldStatusState.Open || session.AccessLevel > Accesslevel.Player)
            {
                DatabaseManager.Shard.GetCharacters(session.AccountID, false, result =>
                {
                    // If you want to create default characters for accounts that have none, here is where you would do it.

                    SendConnectResponse(session, result);
                });
            }
            else
            {
                session.Terminate(SessionTerminationReason.WorldClosed, new GameMessageCharacterError(CharacterError.LogonServerFull));
            }
        }

        private static void SendConnectResponse(Session session, List<Character> characters)
        {
            characters = characters.OrderByDescending(o => o.LastLoginTimestamp).ToList(); // The client highlights the first character in the list. We sort so the first character sent is the one we last logged in
            session.UpdateCharacters(characters);

            GameMessageCharacterList characterListMessage = new GameMessageCharacterList(session.Characters, session);
            GameMessageServerName serverNameMessage = new GameMessageServerName(ConfigManager.Config.Server.WorldName, PlayerManager.GetOnlineCount(), (int)ConfigManager.Config.Server.Network.MaximumAllowedSessions);
            GameMessageDDDInterrogation dddInterrogation = new GameMessageDDDInterrogation();

            session.Network.EnqueueSend(characterListMessage, serverNameMessage);
            session.Network.EnqueueSend(dddInterrogation);
        }
        */
    }
}
