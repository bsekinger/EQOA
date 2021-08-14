using System;
using System.Collections.Generic;
using System.Net;
using ReturnHome.Utilities;

namespace ReturnHome.Server.Network.Managers
{
    public static class SocketManager
    {
        private static ServerListener[] listeners;
		
        public static void Initialize()
        {
			//Hardcoded for the moment, eventually could be read from a config file
			//To create more elaborate designs for the server and zones
			ushort endPoint = 0x73B0;
			ushort port = 10070;
			string serverName = "Default";
			
			//Eventually we would have listeners for multiple ports to support "multiple servers" from same ip?
            var hosts = new List<IPAddress>();

			//Eventually could be a configuration setup for server layout? Hardcode for now
            try
            {
                //Eventually read a config here?
				//Could make an elaborate config setup
				throw new Exception("Forced failure for now");
            }
            catch (Exception ex)
            {
                hosts.Clear();
                hosts.Add(IPAddress.Any);
            }

            listeners = new ServerListener[hosts.Count * 2];

            for (int i = 0; i < hosts.Count; i++)
            {
                listeners[(i * 2)] = new ServerListener(hosts[i], port, endPoint, serverName);
                Logger.Info($"Binding ConnectionListener to {hosts[i]}:{port}");

				//Eventually could add multiple listeners per "server" to distribute packet load? say port 10070/10071 could be one server
                listeners[(i * 2)].StartServer();
            }
        }
    }
}
