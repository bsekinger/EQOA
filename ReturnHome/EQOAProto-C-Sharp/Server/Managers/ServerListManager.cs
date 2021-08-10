using System;
using System.Collections.Concurrent;
using System.Net;

using ReturnHome.Server.Network;
using ReturnHome.Server.Network.GameMessages.Messages;

namespace ReturnHome.Server.Managers
{
    public static class ServerListManager
    {
        private static ConcurrentDictionary<IPEndPoint, Session> sessionDict = new();

        public static void AddSession(Session session)
        {
            if (sessionDict.TryAdd(session.EndPoint, session))
                return;
                Console.WriteLine("Session added to ServerList Queue");

            //else
                Console.WriteLine("Error occured and session was not added to ServerList Queue");
        }

        public static void DistributeServerList()
        {
            //Check to see if there is any point to even send a server list

            if (sessionDict.IsEmpty)
                return;

            //Means there is client's on the server list menu, let's send the list
            var newServerList = new ServerList();

            foreach (var result in sessionDict)
            {
                result.Value.Network.EnqueueSend(newServerList);
            }
        }

        public static void RemoveSession(Session session)
        {
            if (sessionDict.TryRemove(session.EndPoint, out _))
            {
                Console.WriteLine("Session removed from ServerList");
                return;
            }

            Console.WriteLine("Session not removed");
            return;
        }
    }
}
