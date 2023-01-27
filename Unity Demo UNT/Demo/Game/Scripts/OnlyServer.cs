using Unt.Demo.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unt.Demo.Game
{
    public class OnlyServer : MonoBehaviour
    {
        public ushort Port = 12700;

        public Server Server;
        
        private List<uint> clients = new List<uint>();

        private void Start()
        {
            #if UNITY_EDITOR
            Log.Initialize(Debug.Log, includeTimestamps: true);
            #else
            Log.Initialize(Console.WriteLine, includeTimestamps: true);
            #endif
            Console.Clear();

            Server = new Server();

            Server.IsTick = true;

            Server.OnClientConnected = ClientConnected;
            Server.OnClientDisconnected = ClientDisconnected;

            Server.Start(Port);
        }

        private void ClientConnected(uint id)
        {
            Packet packet = Packet.New(DataId.Connected);
            packet.AddByte((byte)clients.Count);

            foreach (var item in clients)
                packet.AddUInt(item);
            
            Server.Send(packet, true, id);
            
            clients.Add(id);
        }

        private void ClientDisconnected(uint id)
        {
            clients.Remove(id);
        }

        private void FixedUpdate()
        {
            Server?.Tick();
        }

        private void OnApplicationQuit()
        {
            Server?.Stop();
        }
    }    
}