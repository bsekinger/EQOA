using System;

namespace ReturnHome.Server.Network
{
    public class ClientPacketMessage : PacketMessage
    {
        public bool Unpack(ReadOnlyMemory<byte> buffer, int offset)
        {
            Header.Unpack(buffer, offset);

			if (buffer.Length < (offset + Header.Size))
				return false;
			
            Data = buffer.Slice(offset, Header.Size);
			offset += Header.Size;
            return true;
        }
    }
}
