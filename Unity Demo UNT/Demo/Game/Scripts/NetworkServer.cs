using Unt.Demo.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace Unt.Demo.Game
{
    public class NetworkServer : MonoBehaviour
    {
        private List<uint> clients = new List<uint>();

        private void Start()
        {
            NetworkManager.Server.OnClientConnected = ClientConnected;
            NetworkManager.Server.OnClientDisconnected = ClientDisconnected;
        }

        private void ClientConnected(uint id)
        {
            Packet packet = Packet.New(DataId.Connected);
            packet.AddByte((byte)clients.Count);

            foreach (var item in clients)
                packet.AddUInt(item);
            
            NetworkManager.Server.Send(packet, true, id);
            
            clients.Add(id);
        }

        private void ClientDisconnected(uint id)
        {
            clients.Remove(id);
        }
    }
}