using System;

namespace Unt
{
    public class NetSegment
    {   
        public byte[] Data;
        public int Length => Data.Length;

        public Action Failed;

        public ushort MaxAttempt;
        public uint Interval;

        public NetSegment() { }

        public NetSegment(Action failed, ushort maxAttempt, uint interval)
        {
            Failed = failed;
            MaxAttempt = maxAttempt;
            Interval = interval;
        }

        public NetSegment AddHeader(NetChennel chennel, params byte[] packets)
        {
            byte[] buffer = new byte[packets.Length + 1];
            buffer[0] = (byte)chennel;

            for (int i = 0; i < packets.Length; i++)
                buffer[i + 1] = packets[i];

            Data = buffer;

            return this;
        }

        public NetSegment AddHeader(NetChennel chennel, byte[] data, int length, params byte[] packets)
        {
            byte[] buffer = new byte[1 + packets.Length + length];
            buffer[0] = (byte)chennel;

            for (int i = 0; i < packets.Length; i++)
                buffer[i + 1] = packets[i];
            
            Array.Copy(data, 0, buffer, packets.Length + 1, length);

            Data = buffer;

            return this;
        }
    }
}