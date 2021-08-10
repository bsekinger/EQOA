using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ReturnHome.Utilities
{
    public static class Utility_Funcs
    {

        public static ulong GetUnixEpoch(this DateTime dateTime)
        {
            var unixTime = dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return (ulong)(unixTime.TotalSeconds);
        }

        ///Performs the actual Technique for us
        public static void DoublePack(BinaryWriter binaryWriter, int value)
        {
            if (value < 0)
                value = Math.Abs(value) * 2 + 1;
            else
                value *= 2;
            binaryWriter.Write7BitEncodedInt(value);
        }

        public static int DoubleUnpack(BinaryReader binaryReader)
        {
            int value = binaryReader.Read7BitEncodedInt();
            return (0 == value % 2 ? value / 2 : ((value - 1) / 2) * -1);
        }

        public static string GetMemoryString(ReadOnlySpan<byte> ClientPacket, int offset, int stringLength)
        {
            return Encoding.Default.GetString(ClientPacket.Slice(offset, stringLength));
        }

        public static void WriteToBuffer<T>(this Memory<byte> buffer, T value, ref int offset) where T : unmanaged
        {
            Span<T> span = MemoryMarshal.CreateSpan(ref value, 1);
            Span<byte> data = MemoryMarshal.AsBytes(span);
            int size = data.Length;
            data.CopyTo(buffer[offset..(offset + size)].Span);
            for (int i = 0; i < size; i++)
            {
                if (data[i] == 0)
                {
                    size = i;
                    break;
                }
            }
            offset += size;
        }
    }

    public class ByteSwaps
    {
        public static ushort SwapBytes(ushort x)
        {
            return (ushort)((ushort)((x & 0xff) << 8) | ((x >> 8) & 0xff));
        }

        public static uint SwapBytes(uint x)
        {
            return (x & 0x000000FFU) << 24 | 
                   (x & 0x0000FF00U) << 8 |
                   (x & 0x00FF0000U) >> 8 | 
                   (x & 0xFF000000U) >> 24;
        }


        public static ulong SwapBytes(ulong value)
        {
            ulong uvalue = value;
            ulong swapped =
                 ((0x00000000000000FF) & (uvalue >> 56)
                 | (0x000000000000FF00) & (uvalue >> 40)
                 | (0x0000000000FF0000) & (uvalue >> 24)
                 | (0x00000000FF000000) & (uvalue >> 8)
                 | (0x000000FF00000000) & (uvalue << 8)
                 | (0x0000FF0000000000) & (uvalue << 24)
                 | (0x00FF000000000000) & (uvalue << 40)
                 | (0xFF00000000000000) & (uvalue << 56));
            return swapped;
        }
    }
}
