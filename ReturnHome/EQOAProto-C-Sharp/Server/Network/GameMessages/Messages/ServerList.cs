using ReturnHome.Utilities;
using System.Configuration;
using System;
using System.Net;
using System.Text;

namespace ReturnHome.Server.Network.GameMessages.Messages
{
    public class ServerList : GameMessage
    {
        public ServerList() : base(MessageType.UnreliableMessage, GameMessageOpcode.ServerList, GameMessageGroup.SecureWeenieQueue)
        {
            var appSettings = ConfigurationManager.AppSettings;
            ///Checks for our "ServerCount" config option
            string result = appSettings["Servercount"] ?? "Not Found";
            int serverCount = int.Parse(result);
            //Utilize double pack to write in total servers
            Utility_Funcs.DoublePack(Writer, serverCount);

            //Read some config for a server list, or maybe a database/server query of some kind
            //Kinda hardcoded for now, *shouldn't* be too much work later to change it's functionality
            for (int i = 0; i < serverCount; i++)
            {
                Pack(appSettings[$"Server{i}"], Convert.ToByte(appSettings[$"ServerRecommended{i}"]), Convert.ToUInt16(appSettings[$"ServerEndPointID{i}"]),
                     Convert.ToUInt16(appSettings[$"ServerPort{i}"]), IPAddress.Parse(appSettings[$"ServerIP{i}"]), Convert.ToByte(appSettings[$"ServerLanguage{i}"]));
            }
        }

        private void Pack(string serverName, byte recommended, ushort serverEndPoint, ushort serverPort, IPAddress serverIP, byte serverLanguage)
        {
            Writer.Write(serverName.Length);
            Writer.Write(Encoding.Unicode.GetBytes(serverName));
            Writer.Write(recommended);
            Writer.Write(serverEndPoint);
            Writer.Write(serverPort);

            byte[] tempbyte = serverIP.GetAddressBytes();

            //Swap bytes for endianess here on the fly
            byte a = tempbyte[0];
            byte b = tempbyte[1];
            tempbyte[0] = tempbyte[3];
            tempbyte[1] = tempbyte[2];
            tempbyte[2] = b;
            tempbyte[3] = a;

            Writer.Write(tempbyte);
            Writer.Write(serverLanguage);
        }
    }
}
