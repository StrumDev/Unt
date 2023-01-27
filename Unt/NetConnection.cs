using System;
using System.Net;

namespace Unt
{
    public class NetConnection
    {
        public ushort Ping { get; private set; }
        public EndPoint ClientEP;
        public NetPeer Peer;

        private NetServer server;

        private DateTime lastTimeOut;
        public bool IsTimeOut => (DateTime.UtcNow - lastTimeOut).TotalMilliseconds > server.TimeOutClient;

        public NetConnection(NetServer server, NetPeer peer, EndPoint endPoint)
        {
            Peer = peer;
            this.server = server;

            ClientEP = endPoint;

            UpdateTimeOut(100);
        }

        public void UpdateTimeOut(ushort rtt)
        {
            lastTimeOut = DateTime.UtcNow;
            Ping = rtt;
            Peer.SetInterval(rtt);
        }
    }
}