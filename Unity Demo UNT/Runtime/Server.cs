using System;
using System.Net;
using System.Collections.Generic;
using Unt;

namespace Unt.Demo.Runtime
{
    public class Server
    {
        public Action<uint> OnClientConnected;
        public Action<uint> OnClientDisconnected;

        public bool IsRuning => server.IsRuning; 
        public bool IsTick = false;

        private NetServer server;
        private Dictionary<EndPoint, uint> clients = new Dictionary<EndPoint, uint>();
        private Dictionary<uint, EndPoint> clientsEP = new Dictionary<uint, EndPoint>();
        private Dictionary<ushort, Action<Packet, uint>> onHandlers = new Dictionary<ushort, Action<Packet, uint>>();

        public Server()
        {
            server = new NetServer();

            server.OnHandler = Handler;
            server.OnClientConnected = ClientConnected;
            server.OnClientDisconnected = ClientDisconnected;
        }
        
        public void AddHandler(ushort dataId, Action<Packet, uint> handler)
        {
            if (!onHandlers.ContainsKey(dataId))
                onHandlers.Add(dataId, handler);
        }
        
        public void Start(ushort port)
        {
            server.Start(port);
        }

        public void Tick() => server.Tick();

        public void Send(Packet packet, bool isReliable, uint clientId)
        {
            packet.Data[0] = (byte)Header.RPC;
            Send(packet, isReliable, clientsEP[clientId]);
        }

        public void SendAll(Packet packet, bool isReliable)
        {
            SendAll(packet, isReliable, null);
        }

        public void SendAll(Packet packet, bool isReliable, uint skip)
        {
            SendAll(packet, isReliable, clientsEP[skip]);
        }

        private void Handler(byte[] data, int length, bool isReliable, EndPoint endPoint)
        {
            Packet packet = new Packet(data, length);
            Header header = (Header)packet.GetByte();

            switch (header)
            {
                case Header.RPC:
                    SendAll(packet, isReliable, endPoint);
                break;
                case Header.Data:
                    DataHandler(packet, clients[endPoint]);
                break;
            }
        }

        private void DataHandler(Packet packet, uint clientId)
        {
            ushort dataId = packet.GetUShort();

            if (onHandlers.TryGetValue(dataId, out var handler))
            {
                if (IsTick) server.AddAction(() => handler(packet, clientId));
                else handler(packet, clientId);
            }   
        }

        private void ClientConnected(EndPoint endPoint)
        {
            uint newId = (uint)endPoint.GetHashCode();
            
            clients.Add(endPoint, newId);
            clientsEP.Add(newId, endPoint);

            Send(Packet.New(Header.Connected).AddUInt(newId), true, endPoint);

            SendAll(Packet.New(Header.ClientConnected).AddUInt(newId), true, endPoint);

            if (IsTick) server.AddAction(() => OnClientConnected(newId));
            else OnClientConnected(newId);
        }

        private void ClientDisconnected(EndPoint endPoint)
        {
            uint id = clients[endPoint];

            clients.Remove(endPoint);
            clientsEP.Remove(id);
            
            SendAll(Packet.New(Header.ClientDisconnected).AddUInt(id), true);
            
            if (IsTick) server.AddAction(() => OnClientDisconnected(id));
            else OnClientDisconnected(id);
        }

        private void Send(Packet packet, bool isReliable, EndPoint endPoint)
        {
            server.Send(packet.Data, packet.Length, isReliable, endPoint);
        }

        private void SendAll(Packet packet, bool isReliable, EndPoint skip = null)
        {
            server.SendAll(packet.Data, packet.Length, isReliable, skip);
        }

        public void Stop()
        {
            server.Stop();
        }
    }
}
