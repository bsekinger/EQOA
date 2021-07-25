  
using System.Threading.Channels;
using System.Threading.Tasks;
using System;
using ReturnHome.PacketProcessing;
using ReturnHome.Server.Network.Managers;
using ReturnHome.Server.Network;
using System.Net;
using ReturnHome.Server.Managers;

namespace ReturnHome.Server
{
    /*
    Keep this in main.cs
    Will also start a thread for packet processing
    */
    public class GameServer
    {
        public static void Main(string[] args)
        {
            //log.Info("Initializing InboundMessageManager...");
            InboundMessageManager.Initialize();

            //"Start worldmanager"
            WorldManager.Initialize();
			
			//Start SocketManager
			SocketManager.Initialize();

            Console.WriteLine("Server has started...");
            Console.ReadLine();
        }
    }
}
