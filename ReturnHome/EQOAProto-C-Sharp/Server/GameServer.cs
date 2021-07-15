  
using System.Threading.Channels;
using System.Threading.Tasks;
using System;
using ReturnHome.PacketProcessing;
using ReturnHome.Server.Network.Managers;
using ReturnHome.Server.Network;
using System.Net;

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
			
			//Start SocketManager
			//SocketManager.Initialize();


            //Tests
            byte[] buffer = new byte[] {0x5a, 0xe7, 0xfe, 0xff, 0xcf, 0xe0, 0x21, 0x5a, 0xe7, 0x05, 0x00, 0x20, 0x01, 0x00, 0xfb, 0x06, 0x01, 0x00, 0x00, 0x00, 0x25,
                                                     0x00, 0x00, 0x00, 0xfb, 0x3e, 0x02, 0x00, 0x04, 0x09, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x45, 0x51, 0x4f,
                                                     0x41, 0x0a, 0x00, 0x00, 0x00, 0x6b, 0x69, 0x65, 0x73, 0x68, 0x61, 0x65, 0x73, 0x68, 0x61, 0x01, 0xfa, 0x10, 0x69, 0x22, 0x1c,
                                                     0xd4, 0x45, 0xbc, 0xfd, 0x68, 0x3c, 0x56, 0x22, 0x87, 0xd9, 0x70, 0xb7, 0x1c, 0x12, 0xae, 0x76, 0xc4, 0x98, 0xfd, 0xf3, 0xce,
                                                     0xeb, 0x44, 0x4a, 0x0a, 0x49, 0xb5, 0xf6, 0xbe, 0x24, 0xe4};
            var packet = new ClientPacket();

            if (packet.Unpack(buffer.AsMemory(), buffer.Length))
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 10070);
                ServerListener stuff = new ServerListener(clientEndPoint.Address, (uint)clientEndPoint.Port);
                Console.WriteLine("Clear...");
                NetworkManager.ProcessPacket(stuff, packet, clientEndPoint);
            }
        }
    }
}
