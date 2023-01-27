using System.Net;
using System.Collections.Generic;
using Timer = System.Timers.Timer;
using System;

namespace Unt
{
    public class NetServer : NetListener
    {
        public uint TimeOutClient = 10000;        
        
        public Action<EndPoint> OnClientConnected;
        public Action<EndPoint> OnClientDisconnected;
        public Action<byte[], int, bool, EndPoint> OnHandler;

        public Dictionary<EndPoint, NetConnection> Connections = new Dictionary<EndPoint, NetConnection>();

        private Timer timer;
        private List<EndPoint> timeOut = new List<EndPoint>();

        public NetServer()
        {
            timer = new Timer();
            timer.Elapsed += (o, e) => UpdateClients();
            timer.AutoReset = true; 

            timer.Interval = 1000;
        }

        public void Start(ushort port)
        {
            if (IsRuning)
                return;

            StartListener(port);

            timer.Start();
            Log.Info($"[Server] Start - {port}");
        }
        
        private void UpdateClients()
        {
            foreach (var client in Connections.Values)
            {
                if (client.IsTimeOut)
                    timeOut.Add(client.ClientEP);
            }

            foreach (var endPoint in timeOut)
                RemoveClient(endPoint);

            timeOut.Clear();
        }

        private bool TryGetClient(byte[] data, EndPoint endPoint)
        {
            if ((NetChennel)data[0] == NetChennel.Connect)
            {
                NetPeer peer = null;

                if (!Connections.ContainsKey(endPoint))
                {
                    Connections.Add(endPoint, new NetConnection(this, peer = new NetPeer(SendTo, endPoint), endPoint));
                    ClientConnected(endPoint);
                }   
                else
                    peer = Connections[endPoint].Peer;
                
                peer.SendAck(NetChennel.Connect);

                return false;
            }

            if (Connections.ContainsKey(endPoint))
                return true;
            
            
            return false;
        }

        public void Send(byte[] data, int length, bool isReliable, EndPoint endPoint)
        {
            if (isReliable)
                SendReliable(data, length, Connections[endPoint].Peer);
            else
                SendUnreliable(data, length, endPoint);
        }

        public void SendAll(byte[] data, int length, bool isReliable, EndPoint skip = null)
        {
            if (isReliable)
            {
                foreach (var client in Connections.Values)
                    if (!client.ClientEP.Equals(skip))
                        SendReliable(data, length, client.Peer);
            }
            else
            {
                foreach (var client in Connections.Keys)
                    if (!client.Equals(skip))
                        SendUnreliable(data, length, client);
            }
        }

        protected override void RawHandler(byte[] data, int length, EndPoint endPoint)
        {
            if (!TryGetClient(data, endPoint))
                return;

            switch ((NetChennel)data[0])
            {
                case NetChennel.Unreliable:
                    HandlerUnreliable(data, length, endPoint);
                break;
                case NetChennel.Reliable:
                    HandlerReliable(data, length, endPoint);
                break;
                case NetChennel.Ack:
                    HandlerAck(data, endPoint);
                break;
                case NetChennel.Disconnect:
                    ClientDisconnected(data, endPoint);
                break;
                case NetChennel.RTT:
                    UpdateRTT(data, endPoint);
                break;
            }
        }

        private void HandlerUnreliable(byte[] data, int length, EndPoint endPoint)
        {
            byte[] buffer = new byte[length - 1];
            Array.Copy(data, 1, buffer, 0, buffer.Length);

            OnHandler?.Invoke(buffer, buffer.Length, false, endPoint);
        }

        private void HandlerReliable(byte[] data, int length, EndPoint endPoint)
        {
            NetPeer peer = Connections[endPoint].Peer;
            
            if (!peer.IsNewAck(data))
                return;

            byte[] buffer = new byte[length - 3];
            Array.Copy(data, 3, buffer, 0, buffer.Length);

            OnHandler?.Invoke(buffer, buffer.Length, true, endPoint);
        }

        private void HandlerAck(byte[] data, EndPoint endPoint)
        {
            NetPeer peer = Connections[endPoint].Peer;

            peer.ClearAck(data);
        }

        private void UpdateRTT(byte[]data, EndPoint endPoint)
        {
            lock (Connections)
            {
                Connections[endPoint].UpdateTimeOut(BitConverter.ToUInt16(data, 1));
                SendTo(NetChennel.RTT, endPoint);
            }
        }

        private void ClientConnected(EndPoint endPoint)
        {
            OnClientConnected?.Invoke(endPoint);
            Log.Info($"[Server] Client Connected {endPoint}");
        }

        private void ClientDisconnected(byte[] data, EndPoint endPoint)
        {
            if (Connections.ContainsKey(endPoint))
                RemoveClient(endPoint);

            SendTo(data, 3, endPoint);
        }
        
        private void RemoveClient(EndPoint endPoint)
        {
            Connections[endPoint].Peer.Close();
            Connections.Remove(endPoint);

            OnClientDisconnected?.Invoke(endPoint);
            
            Log.Info($"[Server] Client Disconnected {endPoint}");
        }

        public void Stop()
        {
            if (!IsRuning)
                return;

            timer.Stop();

            foreach (var client in Connections.Values)
                client.Peer.Close();
            Connections.Clear();

            StopListener();
        }
    }
}