using System;
using System.Net;
using System.Collections.Generic;
using Unt;

namespace Unt.Demo.Runtime
{
    public class Client
    {
        public uint ClientId { get; private set; }
        public ushort Ping => client.Ping;

        public bool IsTick = false;

        public bool IsRuning => client.IsRuning;
        public bool IsNoConnect => client.Status == Status.NoConnect;
        public bool IsConnecting => client.Status == Status.Connecting;
        public bool IsConnected => client.Status == Status.Connected;
        public bool IsDisconnecting => client.Status == Status.Disconnecting;

        public Action<uint> OnConnected;
        public Action OnDisconnected;
        public Action<uint> OnClientConnected;
        public Action<uint> OnClientDisconnected;

        private NetClient client;
        private Dictionary<ushort, Action<Packet>> onHandlers = new Dictionary<ushort, Action<Packet>>();

        public Client()
        {
            client = new NetClient();
            client.OnDisconnected = Disconnected;
            client.OnHandler = Handler;
        }

        public void AddHandler(ushort dataId, Action<Packet> handler)
        {
            if (!onHandlers.ContainsKey(dataId))
                onHandlers.Add(dataId, handler);
        }

        public void Connect(string ip, ushort port)
        {
            client.Connect(ip, port);
        }

        public void Disconnect(bool isReliable = false)
        {
            client.Disconnect(isReliable);
        }

        public void Tick() => client.Tick();

        public void Send(Packet packet, bool isReliable)
        {
            packet.Data[0] = (byte)Header.Data;
            client.Send(packet.Data, packet.Length, isReliable);
        }

        public void Send_RPC(Packet packet, bool isReliable)
        {
            packet.Data[0] = (byte)Header.RPC;
            client.Send(packet.Data, packet.Length, isReliable);
        }

        private void Handler(byte[] data, int length, bool isReliable)
        {
            Packet packet = new Packet(data, length);
            Header header = (Header)packet.GetByte();

            switch (header)
            {
                case Header.RPC:
                    RpcHandler(packet);
                break;
                case Header.Connected:
                    Connected(packet);
                break;
                case Header.ClientConnected:
                    ClientConnected(packet);
                break;
                case Header.ClientDisconnected:
                    ClientDisconnected(packet);
                break;
            }
        }

        private void RpcHandler(Packet packet)
        {
            ushort dataId = packet.GetUShort();

            if (onHandlers.TryGetValue(dataId, out var handler))
            {
                if (IsTick) client.AddAction(() => handler(packet));
                else handler(packet);
            }
        }

        private void Connected(Packet packet)
        {
            uint newId = packet.GetUInt();

            ClientId = newId;

            if (IsTick) client.AddAction(() => OnConnected(newId));
            else OnConnected(newId);
        }

        private void Disconnected()
        {
            if (IsTick) client.AddAction(() => OnDisconnected());
            else OnDisconnected();
        }

        private void ClientConnected(Packet packet)
        {
            uint id = packet.GetUInt();

            if (IsTick) client.AddAction(() => OnClientConnected(id));
            else OnClientConnected(id);
        }

        private void ClientDisconnected(Packet packet)
        {
            uint id = packet.GetUInt();
            
            if (IsTick) client.AddAction(() => OnClientDisconnected(id));
            else OnClientDisconnected(id);
        }

        public void Stop()
        {
            client.Stop();
        }
    }
}
