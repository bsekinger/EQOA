using System;
using System.Buffers.Binary;

namespace ReturnHome.Utilities
{
    class BinaryPrimitiveWrapper
    {
        //Used to slice bytes from readonlymemory
        public static (int, int) GetLEInt(ReadOnlyMemory<byte> mem, int offset)
        {
            offset += 4;
            return (BinaryPrimitives.ReadInt32LittleEndian(mem.Span.Slice(offset - 4, 4)), offset);
        }

        //Used to slice bytes from readonlymemory
        public static (uint, int) GetLEUInt(ReadOnlyMemory<byte> mem, int offset)
        {
            offset += 4;
            return (BinaryPrimitives.ReadUInt32LittleEndian(mem.Span.Slice(offset - 4, 4)), offset);
        }

        //Used to slice bytes from readonlymemory
        public static (uint, int) GetLEUint(ReadOnlyMemory<byte> mem, int offset)
        {
            offset += 4;
            return (BinaryPrimitives.ReadUInt32LittleEndian(mem.Span.Slice(offset - 4, 4)), offset);
        }

        //Used to slice bytes from readonlymemory
        public static (short, int) GetLEShort(ReadOnlyMemory<byte> mem, int offset)
        {
            offset += 2;
            return (BinaryPrimitives.ReadInt16LittleEndian(mem.Span.Slice(offset - 2, 2)), offset);
        }

        //Used to slice bytes from readonlymemory
        public static (ushort, int) GetLEUShort(ReadOnlyMemory<byte> mem, int offset)
        {
            offset += 2;
            return (BinaryPrimitives.ReadUInt16LittleEndian(mem.Span.Slice(offset - 2, 2)), offset);
        }

        //Used to slice bytes from readonlymemory
        public static (byte, int) GetLEByte(ReadOnlyMemory<byte> mem, int offset)
        {
            offset += 1;
            return (mem.Span[offset - 1], offset);
        }
    }
}
