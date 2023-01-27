using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Unt
{
    public delegate void SendTo(byte[] data, EndPoint endPoint);
    public abstract class NetListener
    {
        public bool IsRuning;

        private Socket socket;
        private byte[] buffer;
        
        private bool isClose;
        private object locker = new object();

        private List<Action> actions = new List<Action>();

        protected void StartListener(ushort port = 0)
        {
            lock (locker)
            {
                if (!IsRuning || !isClose)
                {
                    isClose = true;

                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.Bind(new IPEndPoint(IPAddress.Any, port));

                    IsRuning = true;

                    new Thread(new ThreadStart(Receive)).Start();                    
                }
            }
        }

        protected void StopListener()
        {
            lock (locker)
            {
                if (IsRuning || isClose)
                {
                    IsRuning = false;
                    socket.Close();
                    isClose = false;

                    // lock (actions)
                    //     actions.Clear();
                }
            }
        }

        private void Receive()
        {
            EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            buffer = new byte[2048];

            while (socket != null && IsRuning && isClose)
            {
                try
                {
                    if (!IsRuning && socket.Available == 0 && !socket.Poll(500000, SelectMode.SelectRead))
                        continue;
        
                    int size = socket.ReceiveFrom(buffer, ref sender);

                    RawHandler(buffer, size, sender);
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }
        
        protected void SendTo(byte[] data, EndPoint endPoint)
        {
            SendTo(data, data.Length, endPoint);
        }

        protected void SendTo(NetChennel chennel, EndPoint endPoint, params byte[] packets)
        {
            SendTo(chennel, new byte[0], 0, endPoint, packets);
        }

        protected void SendTo(NetChennel chennel, byte[] data, int length, EndPoint endPoint, params byte[] packets)
        {
            byte[] buffer = new byte[1 + packets.Length + length];
            buffer[0] = (byte)chennel;

            for (int i = 0; i < packets.Length; i++)
                buffer[i + 1] = packets[i];
            
            Array.Copy(data, 0, buffer, packets.Length + 1, length);
            
            SendTo(buffer, buffer.Length, endPoint);
        }

        protected void SendTo(byte[] data, int size, EndPoint endPoint)
        {
            socket?.SendTo(data, size, SocketFlags.None, endPoint);
        }
        
        protected abstract void RawHandler(byte[] data, int length, EndPoint endPoint);

        protected void SendUnreliable(byte[] data, int length, EndPoint endPoint)
        {
            SendTo(NetChennel.Unreliable, data, length, endPoint);
        }

        protected void SendReliable(byte[] data, int length, NetPeer peer)
        {
            peer.SendReliable(data, length);
        }

        public void AddAction(Action action)
        {
            lock (actions)
            {
                if (action == null)
                    return;

                actions.Add(action);
            }
        }

        public void Tick()
        {
            lock (actions)
            {
                foreach (var action in actions)
                    action();

                actions.Clear();
            }
        }
    }
}