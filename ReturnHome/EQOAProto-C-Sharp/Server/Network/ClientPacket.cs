using System;

namespace ReturnHome.Server.Network
{
    public class ClientPacket : Packet
    {
        public static int MaxPacketSize { get; } = 1024;

        //Track memory offset as packet is processed
        public int offset = 0;

        public bool Unpack(ReadOnlyMemory<byte> buffer, int bufferSize)
        {
            try
            {
                //Minimum header size... 8
                //Drop if less then that
                if (bufferSize < 8)
                    return false;

                Header.Unpack(buffer, ref offset);

                //Subtract offset from buffersize to verify that the actual packet bundle size is not greater
                if (Header.BundleSize > bufferSize - offset)
                    return false;

                //Need a way to identify an additional bundle from client and to process this

                //If crc checksum is false, crc failed...
				//Verify packet is not a transfer, transfers dont have crc
				//Just drop the packet
                if (!(Header.TargetEndPoint == 0xFFFF))
				{
					if (!Header.CRCChecksum)
						return false;
				
					//If no messages to read, we are done
					if (!ReadMessages(buffer, bufferSize))
						return false;
				}

                else
                {
                    //Process server transfer here
                }

                return true;
            }

            catch (Exception ex)
            {
                //Log exception

                return false;
            }
        }

        private bool ReadMessages(ReadOnlyMemory<byte> buffer, int bufferSize)
        {
			//If messages are present... process
            if (Header.HasBundleFlag(PacketHeaderFlags.NewProcessMessages) || Header.HasBundleFlag(PacketHeaderFlags.ProcessMessageAndReport) ||
				Header.HasBundleFlag(PacketHeaderFlags.ProcessMessages) || Header.HasBundleFlag(PacketHeaderFlags.ProcessAll))
            {
                while (Header.BundleSize != (offset - Header.HeaderSize))
                {
                    try
                    {
                        var message = new ClientPacketMessage();
                        if (!message.Unpack(buffer, offset)) return false;

                        Messages.Add(message);
                    }
					
                    catch (Exception)
                    {
						// Log this?
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
