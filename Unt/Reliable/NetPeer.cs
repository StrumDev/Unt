using System;
using System.Net;
using System.Collections.Generic;

namespace Unt
{
    public class NetPeer
    {
        public SendTo SendTo;
        public EndPoint HostEP;

        public uint Interval = 50;
        public ushort MaxAttempt = 16;

        private NetPending pending;
        private NetPending[] pendings = new NetPending[256];
        private byte pos;

        public NetPeer(SendTo sendTo, EndPoint hostEP)
        {
            SendTo = sendTo;
            HostEP = hostEP;

            pending = new NetPending(this, 0);

            for (int i = 0; i < pendings.Length; i++)
                pendings[i] = new NetPending(this, (byte)i);
        }

        public void SetInterval(ushort rtt)
        {
            uint interval = (uint)Math.Max(10, Math.Round(rtt * 1.2f));

            Interval = interval < 800 ? interval : 800;
        }

        public void SendConnect(ushort maxAttempt, uint interval, Action failed)
        {
            lock (pending)
            {
                pending.AddSegment(new NetSegment(failed, maxAttempt, interval).AddHeader(NetChennel.Connect));
            }
        }

        public void SendDisconnect(ushort maxAttempt, uint interval, Action failed)
        {
            lock (pending)
            {
                pending.AddSegment(new NetSegment(failed, maxAttempt, interval).AddHeader(NetChennel.Disconnect));
            }
        }

        public void SendReliable(byte[] data, int length)
        {
            lock (pendings)
            {
                byte currentPos = ++pos;

                NetSegment segment = new NetSegment(null, MaxAttempt, 0);
                segment.AddHeader(NetChennel.Reliable, data, length, currentPos, ++pendings[currentPos].PacId);
                
                pendings[currentPos].AddSegment(segment);
            }
        }

        public bool IsNewAck(byte[] data)
        {
            NetPending pending = pendings[data[1]];
            
            SendAck(NetChennel.Ack, data[1], data[2]);

            if (pending.AckId != data[2])
            {
                pending.AckId = data[2];
                return true;
            }

            return false;
        }     

        public void ClearAck(byte[] data)
        {
            lock (pendings)
            {
                pendings[data[1]].Clear();
            }
        }

        public void SendAck(NetChennel chennel, params byte[] packets)
        {
            byte[] buffer = new byte[1 + packets.Length];
            buffer[0] = (byte)chennel;

            for (int i = 0; i < packets.Length; i++)
                buffer[i + 1] = packets[i];

            SendTo(buffer, HostEP);
        }
        
        public ushort PendingClear()
        {
            lock (pending)
            {
                return pending.Clear();
            }
        }

        public void Close()
        {
            pending.Cloas();
            
            foreach (NetPending pending in pendings)
                pending.Cloas();
        }
    }
}
