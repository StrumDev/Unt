using Unt.Demo.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace Unt.Demo.Game
{
    public class NetworkClient : MonoBehaviour
    {
        public GameObject PlayerPrefab;
        private Dictionary<uint, Player> players = new Dictionary<uint, Player>();

        private void Start()
        {
            NetworkManager.Client.OnConnected = Connected;
            NetworkManager.Client.OnDisconnected = Disconected;
            NetworkManager.Client.OnClientConnected = ClientConnected;
            NetworkManager.Client.OnClientDisconnected = ClientDisconected;

            NetworkManager.Client.AddHandler(DataId.Connected, Connected);
            NetworkManager.Client.AddHandler(DataId.Move, Move);
        }

        private void Move(Packet packet)
        {
            uint id = packet.GetUInt();

            if (players.TryGetValue(id, out Player player))
            {
                player.Target.position = new Vector3(packet.GetFloat(), packet.GetFloat(), packet.GetFloat());
            }
        }

        private void Connected(uint id)
        {
            Player player = Instantiate(PlayerPrefab).GetComponent<Player>();
            player.ClientId = id;
            player.IsLocal = true;

            players.Add(id, player);
        }

        private void Connected(Packet packet)
        {
            byte count = packet.GetByte();

            for (int i = 0; i < count; i++)
                ClientConnected(packet.GetUInt());
        }

        private void Disconected()
        {
            foreach (var player in players.Values)
                Destroy(player.gameObject);
            players.Clear();
        }

        private void ClientConnected(uint id)
        {
            Player player = Instantiate(PlayerPrefab).GetComponent<Player>();
            player.ClientId = id;
            player.IsLocal = false;

            players.Add(id, player);
        }

        private void ClientDisconected(uint id)
        {
            Destroy(players[id].gameObject);
            players.Remove(id);
        }
    }
}