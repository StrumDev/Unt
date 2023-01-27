using System;
using System.Net;
using Timer = System.Timers.Timer;

namespace Unt
{
    public enum Status : byte { NoConnect, Connecting, Connected, Disconnecting }
    public class NetClient : NetListener
    {
        public ushort Ping { get; private set; }
        public Status Status { get; private set; }

        public uint TimeOutServer = 10000;

        public Action OnConnected;
        public Action OnDisconnected;
        public Action<byte[], int, bool> OnHandler;
        
        private EndPoint serverEP;
        private NetPeer peer;
        private Timer timer;
        private DateTime lastTimeOut;
        private DateTime lastRtt;

        private bool isTimeOut => (DateTime.UtcNow - lastTimeOut).TotalMilliseconds > TimeOutServer;

        public NetClient()
        {
            timer = new Timer();
            timer.Elapsed += (o, e) => UpdateClient();
            timer.AutoReset = true; 

            timer.Interval = 1000;
        }

        public void Connect(string ipAddress, ushort port)
        {
            if (IsRuning)
                return;

            serverEP = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            peer = new NetPeer(SendTo, serverEP);

            StartListener();

            Status = Status.Connecting;

            peer.SendConnect(16, 250, Disconnected);

            lastTimeOut = DateTime.UtcNow;
        }

        public void Disconnect(bool isReliable)
        {
            if (Status == Status.Disconnecting)
                return;
            
            if (Status == Status.Connected)
            {
                if (!isReliable)
                {
                    SendTo(NetChennel.Disconnect, serverEP);
                    Disconnected();
                    return;
                }
                
                Status = Status.Disconnecting;
                peer.SendDisconnect(16, 50, Disconnected);
                return;
                
            }
            else
                Disconnected();
        }

        private void UpdateClient()
        {
            if (Status == Status.Connected)
            {
                if (isTimeOut)
                {
                    Disconnected();
                    return;
                }

                lastRtt = DateTime.UtcNow;
                SendTo(NetChennel.RTT, serverEP, (byte)(Ping >> 0), (byte)(Ping >> 8));
            }
        }

        public void Send(byte[] data, int length, bool isReliable)
        {
            if (isReliable)
                SendReliable(data, length, peer);
            else
                SendUnreliable(data, length, serverEP);
        }
        
        protected override void RawHandler(byte[] data, int length, EndPoint endPoint)
        {
            switch ((NetChennel)data[0])
            {
                case NetChennel.Unreliable:
                    HandlerUnreliable(data, length);
                break;
                case NetChennel.Reliable:
                    HandlerReliable(data, length);
                break;
                case NetChennel.Ack:
                    HandlerAck(data);
                break;
                case NetChennel.Connect:
                    Connected(data);
                break;
                case NetChennel.Disconnect:
                    Disconnected();
                break;
                case NetChennel.RTT:
                    UpdateRTT();
                break;
            }
        }

        private void HandlerUnreliable(byte[] data, int length)
        {
            byte[] buffer = new byte[length - 1];
            Array.Copy(data, 1, buffer, 0, buffer.Length);

            OnHandler?.Invoke(buffer, buffer.Length, false);
        }

        private void HandlerReliable(byte[] data, int length)
        {
            if (!peer.IsNewAck(data))
                return;

            byte[] buffer = new byte[length - 3];
            Array.Copy(data, 3, buffer, 0, buffer.Length);

            OnHandler?.Invoke(buffer, buffer.Length, true);
        }        
        
        private void HandlerAck(byte[] data)
        {
            peer.ClearAck(data);
        }

        private void UpdateRTT()
        {
            Ping = (ushort)(DateTime.UtcNow - lastRtt).TotalMilliseconds;
            lastTimeOut = DateTime.UtcNow;
            peer.SetInterval(Ping);
        }

        private void Connected(byte[] data)
        {
            if (Status != Status.Connected)
            {
                Status = Status.Connected;
                
                Ping = peer.PendingClear();

                timer.Start();

                OnConnected?.Invoke();

                Log.Info("[Client] Connected");
            }
        }

        private void Disconnected()
        {
            if(Status == Status.NoConnect)
                return;
            
            OnDisconnected?.Invoke();
            
            Stop();
            Log.Info($"[Client] Disconnected");
        }

        public void Stop()
        {
            if (!IsRuning)
                return;

            Status = Status.NoConnect;

            timer.Stop();
            peer.Close();

            StopListener();
        }
    }
}