using System;
using System.IO;

namespace ReturnHome.Server.Network
{
    public class ClientPacket : Packet
    {
        public int offset = 0;

        public static int MaxPacketSize { get; } = 1024;

        public bool Unpack(byte[] buffer, int bufferSize)
        {
            //Track memory offset as packet is processed
            try
            {
                //Probably not needed, header length is variable...
                //if (bufferSize < Header.HeaderSize)
                //return false;

                Data = new MemoryStream(buffer, 0, buffer.Length, false, true);
                binaryReader = new BinaryReader(Data);

                Header.Unpack(binaryReader, buffer);

                //Need a way to identify an additional bundle from client and to process this

                //If crc checksum is false, crc failed...
                //Verify packet is not a transfer, transfers dont have crc
                //Just drop the packet
                if (!(Header.TargetEndPoint == 0xFFFF))
                {
                    //If packet indicates to cancel session, do it, and stop reading.
                    //One off issue... Server select -> Character select, if ip/endpoint is the same, client will bundle old session disconnect
                    //in same packet as the new connection, difficult to process this in current setup. Easy to ignore and let client resend
                    if (Header.CancelSession)
                        return true;

                    //If CRC fails and packet isn't canceling the session
                    if (!Header.CRCChecksum)
                        return false;

                    //If the buffer size is equal to bytes read + 4 (CRC)
                    //just return true as packets been fully broke down
                    if (bufferSize == Data.Position + 4)
                        //Packet should just be an ack or session cancel
                        return true;

                    //Read messages, if this fails... drop the packet
                    if (!ReadMessages(bufferSize))
                        return false;
                }

                return true;
            }

            catch (Exception ex)
            {
                //Log exception

                return false;
            }
        }

        private bool ReadMessages(int bufferSize)
        {
            //If message type is present, break out messages
            if (Header.HasBundleFlag(PacketBundleFlags.NewProcessMessages) || Header.HasBundleFlag(PacketBundleFlags.ProcessMessageAndReport) ||
                Header.HasBundleFlag(PacketBundleFlags.ProcessMessages) || Header.HasBundleFlag(PacketBundleFlags.ProcessAll))
            {
                while ((bufferSize - (Data.Position + 4)) != 0)
                {
                    try
                    {
                        var message = new ClientPacketMessage();
                        if (!message.Unpack(binaryReader))
                            return false;

                        Messages.Add(message);
                    }

                    catch (Exception)
                    {
                        Console.WriteLine("Error Splicing Messages from packet");
                        // corrupt packet
                        return false;
                    }
                }
            }

            //No messages present, drop out of packet after verifying ack
            else
                return false;

            return true;
        }

        //Nothing needs to be released... for now.
        public void ReleaseBuffer()
        {

        }
    }
}
