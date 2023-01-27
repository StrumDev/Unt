using System;
using System.Net;
using System.Collections.Generic;
using Timer = System.Timers.Timer;

namespace Unt
{
    internal class NetPending
    {
        public ushort RTT { get; private set; }

        public byte PosId;
        public byte PacId;
        public byte AckId;

        private NetPeer peer;
        private Timer timer; 
        private DateTime lastRtt;

        private List<NetSegment> segments = new List<NetSegment>();
        
        private ushort currentAttempt;
        private bool isSend;

        public NetPending(NetPeer peer, byte posId)
        {
            this.peer = peer;
            PosId = posId;

            timer = new Timer();
            timer.Elapsed += (e, o) => RetrySend();
            timer.AutoReset = false;
        }

        public void AddSegment(NetSegment segment)
        {
            lock (segments)
            {
                segments.Add(segment);

                if (!isSend)
                    RetrySend();
            }
        }

        private void RetrySend()
        {
            lock (segments)
            {
                if (segments.Count == 0)
                    return;

                isSend = true;
                lastRtt = DateTime.UtcNow;

                if (currentAttempt < segments[0].MaxAttempt)
                {
                    TrySend(segments[0]);
                    currentAttempt++;

                    if (currentAttempt > 1)
                        Log.Warning($"RetrySend Current Attempt {currentAttempt}"); 

                    timer.Interval = segments[0].Interval == 0 ? peer.Interval : segments[0].Interval;
                    timer.Start();
                }
                else
                {
                    segments[0].Failed?.Invoke();
                    Clear();
                }
            }
        }

        private void TrySend(NetSegment segment)
        {
            peer.SendTo(segment.Data, peer.HostEP);
        }

        public ushort Clear()
        {
            lock (segments)
            {
                RTT = (ushort)(DateTime.UtcNow - lastRtt).TotalMilliseconds;

                timer.Stop();
                currentAttempt = 0;
                
                if (segments.Count != 0)
                    segments.RemoveAt(0);

                if (segments.Count != 0)
                    RetrySend();
                else
                    isSend = false;
                
                return RTT;
            }
        }

        public void Cloas()
        {
            lock (segments)
            {
                timer.Stop();
                timer.Dispose();
                segments.Clear();
                currentAttempt = 0;
                isSend = false;
            }
        }
    }
}