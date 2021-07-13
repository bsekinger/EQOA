using System;
using System.Collections.Generic;
using System.Net;

using ReturnHome.Server.Network;

///Create a file revolved around UDP to place class into
///
namespace ReturnHome.PacketProcessing
{
    public static class SocketManager
    {
		private static ServerListener[] serverListeners;

		public static void Initialize()
		{
			var servers = new List<IPAddress>();
			
			try
			{
				/*
				Read from config file here for server IP/port info
				May want to consider verifying if ports are open on the PC? 
				Could validate open ports from a pre-expected pool and utilize those going forward, IP would stay the same?
				*/
				
				throw new Exception("Forced failure for now");
				
			}
			
			catch ( Exception ex)
			{
				//Log exception
			
				//Use IPAddress.Any and port 10070
				//Eventually this will just error out and shut the server down
				servers.Add(IPAddress.Any);
			}

            serverListeners = new ServerListener[servers.Count * 2];
			
			for ( int i = 0; i < servers.Count; i++)
			{
                //Eventually ports would vary on these servers
                serverListeners[i] = new ServerListener(servers[i], 10070);
                //Add logging for binding to port
                serverListeners[i].Start();
			}	
		}
	}
}
/*

        ///For now hard code port, config file eventually
        private const int listenPort = 10070;
        public const ushort ServerEndPoint = 0x73B0;
        private readonly int _buffSize = 300;
        private byte[] _buffer = new byte[300];
        public Socket socket;
        //public const int SIO_UDP_CONNRESET = -1744830452;
		
        public async Task StartServer(ChannelWriter<UdpPacketStruct> channelWriter)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
            socket.Bind(new IPEndPoint(IPAddress.Any, listenPort));
            while (true)
            {
                try
                {
                    SocketReceiveFromResult result = await socket.ReceiveFromAsync(
                        new ArraySegment<byte>(_buffer, 0, _buffSize), SocketFlags.None, new IPEndPoint(IPAddress.Any, 0));
                    byte[] buffer = result.ReceivedBytes < _buffSize ? _buffer.AsSpan(0, result.ReceivedBytes).ToArray() : _buffer;
                    channelWriter.TryWrite(new UdpPacketStruct(new Memory<byte>(buffer), (IPEndPoint)result.RemoteEndPoint));
                }

                catch
                {
                    Console.WriteLine("UDP Error");
                }
            }
        }
      
        public void SendPacket(Memory<byte> Packet, IPEndPoint Client)
        {
            try
            {
                SocketAsyncEventArgs asyncSockArgs = new SocketAsyncEventArgs();
                asyncSockArgs.RemoteEndPoint = Client;
                asyncSockArgs.SetBuffer(Packet);
                socket.SendToAsync(asyncSockArgs);
            }

            catch (SocketException e)
            {
                Console.WriteLine($"Received socket error: {e}");
            }
        }
    }
}
*/
